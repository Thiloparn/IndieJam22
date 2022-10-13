using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBetweenImagesScript : MonoBehaviour
{

    public Sprite[] sprites;
    public float duration = .2f;

    float offset = 0f;
    public bool randomOffset = false;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (randomOffset)
        {
            offset = Random.Range(0, sprites.Length);
        }
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sprite = sprites[Mathf.FloorToInt(offset + Time.time / duration) % sprites.Length];
    }
}
