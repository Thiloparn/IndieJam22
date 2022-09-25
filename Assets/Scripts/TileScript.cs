using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

public class TileScript : MonoBehaviour
{

    public BezierSpline trackPath;
    public BezierSpline cameraPath;

    public DirectionEnum[] path;
	
	public int difficulty;

    // Start is called before the first frame update
    void Start()
    {
        /*Vector2Int a = Vector2Int.zero;
        Vector2Int b = Vector2Int.right + Vector2Int.left;
        if (a == b)
        {
            Debug.Log("yes"); // este
        }
        else
        {
            Debug.Log("nop");
        }*/
    }

    // Update is called once per frame
    void Update()
    {

    }
}
