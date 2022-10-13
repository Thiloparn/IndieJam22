using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerScript : MonoBehaviour
{

    public Sprite[] speedSprites;
    public ChapaScript chapaScript;

    public float speed100 = 10f;

    Image image;

    int prevSpr = -1;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        float percent = Mathf.Clamp01(chapaScript.velocity.magnitude / speed100);
        int curSpr = Mathf.FloorToInt(percent * speedSprites.Length);
        if (curSpr == speedSprites.Length)
        {
            curSpr -= 1;
        }
        if (prevSpr != curSpr)
        {
            image.sprite = speedSprites[curSpr];
            prevSpr = curSpr;
        }
    }
}
