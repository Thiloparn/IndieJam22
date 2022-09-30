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
    float trackPosition = -200f;
    Vector3 targetPos = Vector3.zero;
    Quaternion targetRot = Quaternion.identity;

    int dangerLevel = 0;

    bool firstFrame = true;

    public LoseScript lose;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        trackPosition += vel * Time.deltaTime;
        if (trackScript.trackLength <= 0) trackScript.RecalcLength();
        trackPosition = mod(trackPosition, trackScript.trackLength);


        /*int tileIndex = Mathf.FloorToInt(trackPosition);
        TileScript actualTile = trackScript.tiles[tileIndex];
        float actualT = actualTile.trackPath.evenlySpacedPoints.GetNormalizedTAtPercentage(mod(trackPosition, 1f));*/
        Vector3 trackPoint = trackScript.GetPointAtDist(trackPosition);
        Vector3 trackTangent = trackScript.GetTangentAtDist(trackPosition);

        targetPos = Vector3.MoveTowards(targetPos, trackPoint, maxVelMove);
        targetRot = Quaternion.RotateTowards(targetRot, Quaternion.Euler(0, 0, Mathf.Atan2(trackTangent.y, trackTangent.x) * Mathf.Rad2Deg), maxVelDir);

        if (firstFrame)
        {
            targetPos = trackPoint;
            targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(trackTangent.y, trackTangent.x) * Mathf.Rad2Deg);
            firstFrame = false;
        }

        if (!lose.losing)
        {
            transform.position = new Vector3(targetPos.x, targetPos.y, -6f);
            transform.rotation = targetRot;
        }
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
        float chapaPos = trackScript.GetDistAt(closestTile, closestTilePercentage);
        // float chapaPos = trackScript.tiles.IndexOf(closestTile) + closestTilePercentage;
        float otherPos = mod(chapaPos - maxChapaDistance, trackScript.trackLength);

        if (trackPosition > chapaPos && trackPosition < chapaPos + 1f)
        {
            lose.Lose();
            return;
        }

        if (chapaPos > otherPos)
        {
            if (trackPosition > chapaPos)
            {
                trackPosition = otherPos;
            }
            else
            {
                trackPosition = Mathf.Max(trackPosition, otherPos);
            }
        }
        else
        {
            if (trackPosition > chapaPos)
            {
                trackPosition = Mathf.Max(trackPosition, otherPos);
            }
            else
            {
                // nothing
            }
        }

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

    /*int mod(int x, int m)
    {
        return (x % m + m) % m;
    }*/

    float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
