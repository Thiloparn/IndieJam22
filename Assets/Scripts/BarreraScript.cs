using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarreraScript : MonoBehaviour
{

    public Chapa2Script chapaScript;
    public BeatManager beatManager;
    public TrackScript trackScript;

    public float maxVelMove = .8f;
    public float maxVelDir = 20f;

    public float maxChapaDistance = 2f;
    public float vel = .25f;
    // float distanceToChapa = .8f;
    float trackPosition = -10f;
    Vector3 targetPos = Vector3.zero;
    Quaternion targetRot = Quaternion.identity;

    int dangerLevel = 0;

    bool firstFrame = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        trackPosition += vel * Time.deltaTime;
        trackPosition = mod(trackPosition, trackScript.tiles.Count);
        int tileIndex = Mathf.FloorToInt(trackPosition);
        TileScript actualTile = trackScript.tiles[tileIndex];
        float actualT = actualTile.trackPath.evenlySpacedPoints.GetNormalizedTAtPercentage(mod(trackPosition, 1f));
        targetPos = Vector3.MoveTowards(targetPos, actualTile.trackPath.GetPoint(actualT), maxVelMove);
        Vector3 tangent = actualTile.trackPath.GetTangent(actualT);
        targetRot = Quaternion.RotateTowards(targetRot, Quaternion.Euler(0, 0, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg), maxVelDir);

        if (firstFrame)
        {
            targetPos = actualTile.trackPath.GetPoint(actualT);
            targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
            firstFrame = false;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y, -6f);
        transform.rotation = targetRot;
    }

    public void BeatMiss()
    {
        /*if (dangerLevel == 0)
        {
            dangerLevel = 1;
            distanceToChapa = 0.2f;
            Invoke("LowerDanger", 1.5f);
        }
        else
        {
            Debug.Log("LOST!");
            distanceToChapa = 0.001f;
        }*/
    }

    void LowerDanger()
    {
        /*if (dangerLevel == 1)
        {
            dangerLevel = 0;
            distanceToChapa = .8f;
        }*/
    }

    public void SetChapaPos(TileScript closestTile, float closestTilePercentage)
    {
        trackPosition = Mathf.Max(trackPosition, trackScript.tiles.IndexOf(closestTile) + closestTilePercentage - maxChapaDistance);
        /*
        if (closestTilePercentage >= distanceToChapa)
        {
            TileScript actualTile = closestTile;
            float actualT = closestTile.trackPath.evenlySpacedPoints.GetNormalizedTAtPercentage(closestTilePercentage - distanceToChapa);
            targetPos = Vector3.MoveTowards(targetPos, closestTile.trackPath.GetPoint(actualT), maxVelMove);
            Vector3 tangent = actualTile.trackPath.GetTangent(actualT);
            targetRot = Quaternion.RotateTowards(targetRot, Quaternion.Euler(0, 0, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg), maxVelDir);
        }
        else
        {
            int tileIndex = trackScript.tiles.IndexOf(closestTile);
            TileScript actualTile = trackScript.tiles[mod(tileIndex - 1, trackScript.tiles.Count)];
            float actualT = actualTile.trackPath.evenlySpacedPoints.GetNormalizedTAtPercentage(1f + closestTilePercentage - distanceToChapa);
            targetPos = Vector3.MoveTowards(targetPos, actualTile.trackPath.GetPoint(actualT), maxVelMove);
            Vector3 tangent = actualTile.trackPath.GetTangent(actualT);
            targetRot = Quaternion.RotateTowards(targetRot, Quaternion.Euler(0, 0, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg), maxVelDir);
        }
        transform.position = new Vector3(targetPos.x, targetPos.y, -6f);
        transform.rotation = targetRot;
        */
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
