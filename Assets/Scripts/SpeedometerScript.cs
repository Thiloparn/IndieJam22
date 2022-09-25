using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedometerScript : MonoBehaviour
{

    public Sprite[] speedSprites;
    public Chapa2Script chapaScript;

    public float speed100 = 10f;

    SpriteRenderer spriteRenderer;

    int prevSpr = -1;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            spriteRenderer.sprite = speedSprites[curSpr];
            prevSpr = curSpr;
        }
    }
}
