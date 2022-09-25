using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionEnum { RIGHT, UP, LEFT, DOWN }

struct Corner
{
    public Vector2Int pos;
    public int distanceToNext;

    public Corner(Vector2Int pos, int distanceToNext)
    {
        this.pos = pos;
        this.distanceToNext = distanceToNext;
    }

}

public class GeneratorScript : MonoBehaviour
{

    public int maxWidth = 10;
    public int maxHeight = 10;

    Vector2Int[,] nextTileDir;
    bool[,] isDone;

    public GameObject[] tilePrefabs;

    public TrackScript track;

    public int complexity = 100;
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

        List<Corner> corners = new List<Corner>();

        corners.Add(new Corner(new Vector2Int(0, 0), maxWidth - 1));
        corners.Add(new Corner(new Vector2Int(maxWidth - 1, 0), maxHeight - 1));
        corners.Add(new Corner(new Vector2Int(maxWidth - 1, maxHeight - 1), maxWidth - 1));
        corners.Add(new Corner(new Vector2Int(0, maxHeight - 1), maxHeight - 1));


        // debug
        // nextTileDir[0, 0] = Vector2Int.zero;
        // nextTileDir[0, 1] = Vector2Int.right;
        // nextTileDir[1, 1] = Vector2Int.down;

        Vector2Int tempInnerPoint;

        int complexityIters = 0;
        while (complexityIters < complexity)
        {
            complexityIters++;

            // cut a corner
            int cutCornerIndex = Random.Range(0, corners.Count);
            // int cutCornerIndex = 1;
            Corner cutCorner = corners[cutCornerIndex];
            Corner prevCorner = corners[mod(cutCornerIndex - 1, corners.Count)];

            if (cutCorner.distanceToNext <= 1 || prevCorner.distanceToNext <= 1) continue;

            Vector2Int prevDir = nextTileDir[prevCorner.pos.x, prevCorner.pos.y];
            Vector2Int nextDir = nextTileDir[cutCorner.pos.x, cutCorner.pos.y];

            int prevDistance = Random.Range(1, prevCorner.distanceToNext);
            int nextDistance = Random.Range(1, cutCorner.distanceToNext);
            // int prevDistance = 1;
            // int nextDistance = 2;

            bool validCut = true;
            for (int i = 1; i < prevDistance; i++)
            {
                for (int j = 1; j < nextDistance; j++)
                {
                    tempInnerPoint = cutCorner.pos - prevDir * i + nextDir * j;
                    if (nextTileDir[tempInnerPoint.x, tempInnerPoint.y] != Vector2Int.zero)
                    {
                        validCut = false;
                        break;
                    }
                }
                if (!validCut) break;
            }
            if (!validCut) continue;

            // Debug.Log("cutting");

            for (int i = 0; i < prevDistance; i++)
            {
                tempInnerPoint = cutCorner.pos - prevDir * i;
                nextTileDir[tempInnerPoint.x, tempInnerPoint.y] = Vector2Int.zero;
                // Debug.Log("i1: setting " + tempInnerPoint.ToString() + " to zero");
            }
            for (int j = 0; j < nextDistance; j++)
            {
                tempInnerPoint = cutCorner.pos + nextDir * j;
                nextTileDir[tempInnerPoint.x, tempInnerPoint.y] = Vector2Int.zero;
                // Debug.Log("j1: setting " + tempInnerPoint.ToString() + " to zero");
            }

            for (int i = 1; i <= prevDistance; i++)
            {
                tempInnerPoint = cutCorner.pos - prevDir * i + nextDir * nextDistance;
                nextTileDir[tempInnerPoint.x, tempInnerPoint.y] = prevDir;
                // Debug.Log("i2: setting " + tempInnerPoint.ToString() + " to " + prevDir.ToString());
            }
            for (int j = 0; j < nextDistance; j++)
            {
                tempInnerPoint = cutCorner.pos + nextDir * j - prevDir * prevDistance;
                nextTileDir[tempInnerPoint.x, tempInnerPoint.y] = nextDir;
                // Debug.Log("j2: setting " + tempInnerPoint.ToString() + " to " + nextDir.ToString());
            }

            Corner newPrevCorner = new Corner(cutCorner.pos - prevDir * prevDistance, nextDistance);
            Corner newNextCorner = new Corner(cutCorner.pos + nextDir * nextDistance, cutCorner.distanceToNext - nextDistance);

            corners[mod(cutCornerIndex - 1, corners.Count)] = new Corner(prevCorner.pos, prevCorner.distanceToNext - prevDistance);
            corners[cutCornerIndex] = new Corner(cutCorner.pos + nextDir * nextDistance - prevDir * prevDistance, prevDistance);
            corners.Insert(cutCornerIndex + 1, newNextCorner);
            corners.Insert(cutCornerIndex, newPrevCorner);

            // for (int k = 0; k < corners.Count; k++)
            // {
            //     Debug.Log("corner " + k + ": pos is " + corners[k].pos.ToString() + ", dist is " + corners[k].distanceToNext);
            // }

            // break;
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

        // Debug.Log("starting tile: " + prevTile.ToString());

        int maxPrefabPathLength = 0;
        TileScript[] prefabTileScripts = new TileScript[tilePrefabs.Length];
        for (int k = 0; k < tilePrefabs.Length; k++)
        {
            prefabTileScripts[k] = tilePrefabs[k].GetComponent<TileScript>();
            maxPrefabPathLength = Mathf.Max(maxPrefabPathLength, prefabTileScripts[k].path.Length);
        }

        // avoid declaring variables in loop
        int validPrefabsCount;
        GameObject[] validPrefabs = new GameObject[tilePrefabs.Length * 2];
        bool[] validPrefabsFlip = new bool[tilePrefabs.Length * 2];
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

            // Debug.Log("starting new thing, at initTile: " + initTile.ToString());
            curTile = initTile;
            curSegmentLength = 1 + Random.Range(0, maxPrefabPathLength);
            for (int k = 0; k < curSegmentLength; k++)
            {
                // Debug.Log("curTile: " + curTile.ToString());
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
                    validPrefabsFlip[validPrefabsCount] = false;
                    validPrefabs[validPrefabsCount] = tilePrefabs[n];
                    validPrefabsCount++;
                }

                if (isInvertedCompatible(curSegmentLength, curSegment, prefabTileScripts[n].path))
                {
                    validPrefabsFlip[validPrefabsCount] = true;
                    validPrefabs[validPrefabsCount] = tilePrefabs[n];
                    validPrefabsCount++;
                }
            }

            if (validPrefabsCount == 0)
            {
                continue;
            }

            //             // Quaternion.FromToRotation(Vector3.right, new Vector3(initDirection.x, initDirection.y, 0f))

            int prefabIndex = Random.Range(0, validPrefabsCount);
            GameObject cur = GameObject.Instantiate(
                validPrefabs[prefabIndex],
                new Vector3(initTile.x * 32, initTile.y * 32, 10f),
                rotFromInitDir(initDirection),
                track.transform
            );
            if (validPrefabsFlip[prefabIndex])
            {
                cur.transform.localScale = new Vector3(cur.transform.localScale.x, -cur.transform.localScale.y, cur.transform.localScale.z);
            }
            track.tiles.Add(cur.GetComponent<TileScript>());

            curTile = initTile;
            for (int k = 0; k < curSegmentLength; k++)
            {
                Vector2Int curDir = nextTileDir[curTile.x, curTile.y];
                isDone[curTile.x, curTile.y] = true;
                if (k == curSegmentLength - 1)
                {
                    prevTile = curTile;
                }
                Debug.Log(curTile.ToString());
                curTile += curDir;
            }

            if (isDone[curTile.x, curTile.y])
            {
                done = true;
                break;
            }

            // Debug.Log("correctly placed a thing; tile init was " + initTile.ToString() + ", prevTile is now " + prevTile.ToString());
        }
        track.transform.position -= new Vector3(track.tiles[0].transform.position.x, track.tiles[0].transform.position.y, 0f);
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

    bool isInvertedCompatible(int segmentLength, Vector2Int[] realSegment, DirectionEnum[] prefabSegment)
    {
        if (prefabSegment.Length != segmentLength)
        {
            return false;
        }
        bool anyVert = false;
        for (int k = 0; k < segmentLength; k++)
        {
            switch (prefabSegment[k])
            {
                case DirectionEnum.RIGHT:
                    if (realSegment[k] != Vector2Int.right) return false;
                    break;
                case DirectionEnum.UP:
                    if (realSegment[k] != Vector2Int.down) return false;
                    anyVert = true;
                    break;
                case DirectionEnum.LEFT:
                    if (realSegment[k] != Vector2Int.left) return false;
                    break;
                case DirectionEnum.DOWN:
                    if (realSegment[k] != Vector2Int.up) return false;
                    anyVert = true;
                    break;
            }
        }
        // if (!anyVert) return false; // allow flipping of purely horizontal sprites?
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

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
