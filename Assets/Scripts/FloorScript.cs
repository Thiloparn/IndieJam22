using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorScript : MonoBehaviour
{

    [Range(0f, 20f)]
    public float drag = 0f;

    public FloorType type;

    public bool isHole = false;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Floors"); ;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
