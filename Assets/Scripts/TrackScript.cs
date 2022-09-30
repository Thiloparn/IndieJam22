using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackScript : MonoBehaviour
{

    public List<TileScript> tiles;

    public float trackLength = -1;

    public Sprite[] grassSprites;
    public float grassSpacing = 2f;
    public float grassDist = 8.61f;

    // Start is called before the first frame update
    void Start()
    {
        if (grassSprites.Length > 0)
        {
            Invoke("AddGrass", 0.0001f);
        }
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

    public void AddGrass()
    {
        for (float d = 0; d < trackLength; d += grassSpacing)
        {
            Vector3 trackPoint = GetPointAtDist(d);
            Vector3 trackTangent = GetTangentAtDist(d);
            float angle = Mathf.Atan2(trackTangent.y, trackTangent.x) * Mathf.Rad2Deg;

            GameObject grass1 = new GameObject("grass");
            SpriteRenderer spr1 = grass1.AddComponent<SpriteRenderer>();
            spr1.sprite = grassSprites[Random.Range(0, grassSprites.Length)];
            grass1.transform.position = trackPoint + new Vector3(-trackTangent.y, trackTangent.x, Random.Range(1f, 2f)) * grassDist;
            grass1.transform.rotation = Quaternion.Euler(0, 0, angle);

            GameObject grass2 = new GameObject("grass");
            SpriteRenderer spr2 = grass2.AddComponent<SpriteRenderer>();
            spr2.flipX = true;
            spr2.sprite = grassSprites[Random.Range(0, grassSprites.Length)];
            grass2.transform.position = trackPoint - new Vector3(-trackTangent.y, trackTangent.x, -Random.Range(1f, 2f)) * grassDist;
            grass2.transform.rotation = Quaternion.Euler(0, 0, 180f + angle);
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
