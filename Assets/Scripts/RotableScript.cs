using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotableScript : MonoBehaviour
{

    public GameObject startsGoingRight;
    public GameObject startsGoingUp;
    public GameObject startsGoingLeft;
    public GameObject startsGoingDown;

    // Start is called before the first frame update
    void Start()
    {
        startsGoingRight.SetActive(false);
        startsGoingUp.SetActive(false);
        startsGoingDown.SetActive(false);
        startsGoingLeft.SetActive(false);

        Vector3 vecThing = transform.rotation * Vector3.right;
        if (Mathf.Abs(vecThing.x) > Mathf.Abs(vecThing.y))
        {
            if (vecThing.x > 0)
            {
                startsGoingRight.SetActive(true);
            }
            else
            {
                startsGoingLeft.SetActive(true);
            }
        }
        else
        {
            if (vecThing.y > 0)
            {
                startsGoingUp.SetActive(true);
            }
            else
            {
                startsGoingDown.SetActive(true);
            }
        }

        if (transform.lossyScale.x < 0f)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        if (transform.lossyScale.y < 0f)
        {
            transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
