using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public ChapaScript player;

    Camera cam;

    public bool useMargin = true;
    [Tooltip("How close the player can get to the border.")]
    [Range(0.0f, 0.5f)]
    public float margin;

    Vector3 pathPos = Vector3.zero;

    public float maxSpeed = .1f;

    // public bool predictPosition = false;
    // public float predictDistance = 0.5f;
    // [Tooltip("The lower this is, the less the camera will respond to sudden changes in direction.")]
    // public float predictTurnSpeed = 0.5f;
    // [Tooltip("How fast the camera moves to its target position.")]
    // public float predictMoveSpeed = 0.5f;
    // Vector2 predictDelta = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void SetPathPos(Vector3 pos)
    {
        pathPos = Vector3.MoveTowards(pathPos, pos, maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        // if (predictPosition)
        // {
        //     // predictDelta is the current velocity with some "inertia", so sudden changes in direction don't move the camera too much
        //     // predictDelta = Vector2.MoveTowards(predictDelta.normalized, player.velocity.normalized, predictTurnSpeed);

        //     predictDelta = Vector2.MoveTowards(predictDelta, player.mouseDirection, predictTurnSpeed);

        //     Vector2 targetPosition = player.position + predictDelta * predictDistance;
        //     transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), predictMoveSpeed);
        // }

        transform.position = new Vector3(pathPos.x, pathPos.y, transform.position.z);
        // transform.position = Vector3.MoveTowards(transform.position, new Vector3(pathPos.x, pathPos.y, transform.position.z), maxSpeed);

        if (useMargin)
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
}
