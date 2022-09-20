using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public ChapaScript player;

    Camera cam;

    [Tooltip("How close the player can get to the border.")]
    [Range(0.0f, 0.5f)]
    public float margin;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 actualPlayerPos = player.transform.position;
        Vector3 viewPos = cam.WorldToScreenPoint(actualPlayerPos);
        viewPos /= cam.pixelRect.size;
        viewPos.x = Mathf.Clamp(viewPos.x, margin, 1f - margin);
        viewPos.y = Mathf.Clamp(viewPos.y, margin, 1f - margin);
        viewPos *= cam.pixelRect.size;
        viewPos.z = Mathf.Abs(actualPlayerPos.z - cam.transform.position.z);
        Vector3 idealPlayerPos = cam.ScreenToWorldPoint(viewPos);
        transform.position += (actualPlayerPos - idealPlayerPos);
    }
}
