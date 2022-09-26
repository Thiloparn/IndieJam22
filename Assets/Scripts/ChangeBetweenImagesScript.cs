using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBetweenImagesScript : MonoBehaviour
{

    public Sprite[] sprites;
    public float duration = .2f;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sprite = sprites[Mathf.FloorToInt(Time.time / duration) % sprites.Length];
    }
}
