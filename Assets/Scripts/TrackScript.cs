using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackScript : MonoBehaviour
{

    public List<TileScript> tiles;

    public float trackLength = -1;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void RecalcLength()
    {
        Debug.Log("recalquing length");
        trackLength = 0;
        foreach (TileScript tile in tiles)
        {
            trackLength += tile.trackPath.length;
        }
    }

    public Vector3 GetPointAtDist(float dist)
    {
        foreach (TileScript tile in tiles)
        {
            if (dist < tile.trackPath.length)
            {
                return tile.trackPath.pointCache.GetPoint(dist / tile.trackPath.length);
            }
            else
            {
                dist -= tile.trackPath.length;
            }
        }
        Debug.LogError("bad dist: " + dist);
        return Vector3.zero;
    }

    public Vector3 GetTangentAtDist(float dist)
    {
        foreach (TileScript tile in tiles)
        {
            if (dist < tile.trackPath.length)
            {
                return tile.trackPath.pointCache.GetTangent(dist / tile.trackPath.length);
            }
            else
            {
                dist -= tile.trackPath.length;
            }
        }
        Debug.LogError("bad dist: " + dist);
        return Vector3.zero;
    }

    public float GetDistAt(TileScript closestTile, float closestTilePercentage)
    {
        float result = 0f;
        foreach (TileScript tile in tiles)
        {
            if (tile == closestTile)
            {
                result += closestTilePercentage * tile.trackPath.length;
                return result;
            }
            else
            {
                result += tile.trackPath.length;
            }
        }
        Debug.LogError("bad tile: " + closestTile);
        return 0;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
