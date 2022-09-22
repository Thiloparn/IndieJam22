using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{

    [Range(0f, 2f)]
    public float bounciness = 1f;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Walls"); ;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
