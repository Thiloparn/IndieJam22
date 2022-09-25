using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionEnum { RIGHT, UP, LEFT, DOWN }

public class GeneratorScript : MonoBehaviour
{

    public int maxWidth = 10;
    public int maxHeight = 10;

    Vector2Int[,] nextTileDir;
    bool[,] isDone;

    public GameObject[] tilePrefabs;

    public TrackScript track;

    public int debugMaxTries = 10;

    // Start is called before the first frame update
    void Start()
    {
        // STEP 1: generate circuit

        // counter clockwise
        nextTileDir = new Vector2Int[maxWidth, maxHeight];
        for (int i = 1; i < maxWidth; i++)
        {
            nextTileDir[i - 1, 0] = Vector2Int.right;
            nextTileDir[i, maxHeight - 1] = Vector2Int.left;
        }
        for (int j = 1; j < maxHeight; j++)
        {
            nextTileDir[0, j] = Vector2Int.down;
            nextTileDir[maxWidth - 1, j - 1] = Vector2Int.up;
        }

        // STEP 2: convert it to tiles
        isDone = new bool[maxWidth, maxHeight];
        Vector2Int prevTile = Vector2Int.zero;
        while (nextTileDir[prevTile.x, prevTile.y] == Vector2Int.zero)
        {
            prevTile.x += 1;
            if (prevTile.x >= maxWidth)
            {
                prevTile.x = 0;
                prevTile.y += 1;
            }
        }

        Debug.Log("starting tile: " + prevTile.ToString());

        int maxPrefabPathLength = 0;
        TileScript[] prefabTileScripts = new TileScript[tilePrefabs.Length];
        for (int k = 0; k < tilePrefabs.Length; k++)
        {
            prefabTileScripts[k] = tilePrefabs[k].GetComponent<TileScript>();
            maxPrefabPathLength = Mathf.Max(maxPrefabPathLength, prefabTileScripts[k].path.Length);
        }

        // avoid declaring variables in loop
        int validPrefabsCount;
        GameObject[] validPrefabs = new GameObject[tilePrefabs.Length];
        Vector2Int initDirection;
        Vector2Int initTile;
        Vector2Int curTile;
        int curSegmentLength;
        Vector2Int[] curSegment = new Vector2Int[maxPrefabPathLength];

        int nTries = 0;
        bool done = false;
        while (!done && nTries < debugMaxTries)
        {
            nTries++;

            initDirection = nextTileDir[prevTile.x, prevTile.y];
            initTile = prevTile + initDirection;

            Debug.Log("starting new thing, at initTile: " + initTile.ToString());
            curTile = initTile;
            curSegmentLength = 1 + Random.Range(0, maxPrefabPathLength);
            for (int k = 0; k < curSegmentLength; k++)
            {
                Debug.Log("curTile: " + curTile.ToString());
                /*if (isDone[curTile.x, curTile.y])
                {
                    done = true;
                    break;
                }*/
                Vector2Int curDir = nextTileDir[curTile.x, curTile.y];
                curSegment[k] = rotate(curDir, initDirection);
                curTile += curDir;
            }
            if (done)
            {
                break;
            }

            validPrefabsCount = 0;
            for (int n = 0; n < tilePrefabs.Length; n++)
            {
                if (isCompatible(curSegmentLength, curSegment, prefabTileScripts[n].path))
                {
                    validPrefabs[validPrefabsCount] = tilePrefabs[n];
                    validPrefabsCount++;
                }
            }

            if (validPrefabsCount == 0)
            {
                continue;
            }

            //             // Quaternion.FromToRotation(Vector3.right, new Vector3(initDirection.x, initDirection.y, 0f))

            track.tiles.Add(GameObject.Instantiate(
                validPrefabs[Random.Range(0, validPrefabsCount)],
                new Vector3(initTile.x * 32, initTile.y * 32, 10f),
                rotFromInitDir(initDirection)
            ).GetComponent<TileScript>());

            curTile = initTile;
            for (int k = 0; k < curSegmentLength; k++)
            {
                Vector2Int curDir = nextTileDir[curTile.x, curTile.y];
                isDone[curTile.x, curTile.y] = true;
                if (k == curSegmentLength - 1)
                {
                    prevTile = curTile;
                }
                curTile += curDir;
            }

            if (isDone[curTile.x, curTile.y])
            {
                done = true;
                break;
            }

            Debug.Log("correctly placed a thing; tile init was " + initTile.ToString() + ", prevTile is now " + prevTile.ToString());
        }
    }

    Quaternion rotFromInitDir(Vector2Int dir)
    {
        if (dir == Vector2Int.right)
        {
            return Quaternion.identity;
        }
        else if (dir == Vector2Int.left)
        {
            return Quaternion.Euler(0f, 0f, 180f);
        }
        else if (dir == Vector2Int.up)
        {
            return Quaternion.Euler(0f, 0f, 90f);
        }
        else
        {
            return Quaternion.Euler(0f, 0f, 270f);
        }

    }

    bool isCompatible(int segmentLength, Vector2Int[] realSegment, DirectionEnum[] prefabSegment)
    {
        if (prefabSegment.Length != segmentLength)
        {
            return false;
        }
        for (int k = 0; k < segmentLength; k++)
        {
            if (realSegment[k] != dirToVec(prefabSegment[k]))
            {
                return false;
            }
        }
        return true;
    }

    Vector2Int dirToVec(DirectionEnum dir)
    {
        switch (dir)
        {
            case DirectionEnum.RIGHT:
                return Vector2Int.right;
            case DirectionEnum.UP:
                return Vector2Int.up;
            case DirectionEnum.LEFT:
                return Vector2Int.left;
            case DirectionEnum.DOWN:
                return Vector2Int.down;
        }
        Debug.LogError("invalid dir");
        return Vector2Int.right;
    }

    // rotate direction so that the same rotation will turn initDirection into Vector2Int.right
    Vector2Int rotate(Vector2Int direction, Vector2Int initDirection)
    {
        if (initDirection == Vector2Int.right)
        {
            return direction;
        }
        else if (initDirection == Vector2Int.left)
        {
            return -direction;
        }
        else if (initDirection == Vector2Int.up)
        {
            return new Vector2Int(direction.y, -direction.x);
        }
        else
        {
            return new Vector2Int(-direction.y, direction.x);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
