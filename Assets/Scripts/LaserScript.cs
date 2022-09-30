using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour
{

    public float timeOn = 1f;
    public float timeOff = .5f;
    public float offset = .1f;

    public GameObject laserOn;
    public GameObject laserOff;

    bool wasPreviousOn = false;


    // Start is called before the first frame update
    void Start()
    {
        if (timeOff <= 0f)
        {
            laserOn.SetActive(true);
            laserOff.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeOff > 0f)
        {
            float thing = (Time.time + offset) % (timeOn + timeOff);
            Debug.Log(thing);
            if (thing < timeOn)
            {
                if (!wasPreviousOn)
                {
                    wasPreviousOn = true;
                    laserOn.SetActive(true);
                    laserOff.SetActive(false);
                }
            }
            else
            {
                if (wasPreviousOn)
                {
                    wasPreviousOn = false;
                    laserOn.SetActive(false);
                    laserOff.SetActive(true);
                }
            }
        }
    }
}
