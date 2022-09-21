using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapaScript : MonoBehaviour
{

    [HideInInspector]
    public Vector2 position;
    [HideInInspector]
    public Vector2 velocity;

    [Range(0.01f, 2.0f)]
    public float secondsBetweenBeats;

    [Range(0.0f, 500.0f)]
    public float accelerationAmount;

    [Range(0f, 1.0f)]
    [Tooltip("If greater than 0, it will apply the accelerationAmount a lo largo de esta cantidad de segundos.")]
    public float accelerationTime;

    [Range(0.0f, 10.0f)]
    public float floorDrag;

    [Tooltip("If true, act as if mouse was pressed every beat.")]
    public bool alwaysPressed;

    [Tooltip("If true, right mouse button will stop the chapa.")]
    public bool allowBrake;

    [Tooltip("How much velocity is maintained after bouncing.")]
    [Range(0.0f, 2.0f)]
    public float bounciness;

    [Tooltip("If true, the chapa will be pushed by the mouse instead of pulled.")]
    public bool invertDirection = false;

    [Range(0.0f, 2.0f)]
    public float airTime;

    [Range(0.0f, 10.0f)]
    public float airDrag;

    float remainingTimeOnAir = 0f;
    float timeSinceLastBeat = 0f;

    Vector2 lastDirection;

    // temp variables, for debugging
    public Color backgroundMainColor;
    public Color backgroundBeatColor;

    float chapaRadius;

    // Start is called before the first frame update
    void Start()
    {
        // dirty hack
        chapaRadius = transform.localScale.x / 2f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        remainingTimeOnAir = Mathf.Max(0f, remainingTimeOnAir - Time.deltaTime);

        timeSinceLastBeat += Time.deltaTime;
        if (timeSinceLastBeat > secondsBetweenBeats)
        {
            timeSinceLastBeat %= secondsBetweenBeats;
            Beat();
        }

        Camera.main.backgroundColor = Color.Lerp(backgroundBeatColor, backgroundMainColor, (timeSinceLastBeat / secondsBetweenBeats) * (timeSinceLastBeat / secondsBetweenBeats));

        if (accelerationTime > 0f && timeSinceLastBeat < accelerationTime)
        {
            velocity += accelerationAmount * lastDirection * Time.deltaTime / accelerationTime;
        }

        float curDrag = (remainingTimeOnAir > 0f) ? airDrag : floorDrag;
        velocity -= velocity * curDrag * Time.deltaTime;

        // Bounce against walls!
        float deltaDistance = velocity.magnitude * Time.deltaTime;
        // todo: change to the "nonalloc" version
        RaycastHit2D hit = Physics2D.CircleCast(position, chapaRadius, velocity, deltaDistance);

        position += velocity * Time.deltaTime;

        // while (hit.collider != null)
        if (hit.collider != null)
        {
            velocity = Vector2.Reflect(velocity, hit.normal) * bounciness;
            if (hit.distance == 0f)
            {
                // we might be inside a thing
                if (Vector2.Dot(velocity, hit.point - hit.centroid) > 0f)
                {
                    velocity *= -1f;
                }
            }
            // Debug.Assert(hit.distance == Vector2.Distance(hit.centroid, position));
            // position = hit.centroid + velocity.normalized * 0.01f;
            position = hit.centroid + velocity.normalized * (deltaDistance - hit.distance);
            /*
            // sometimes, multiple bounces in a single frame (ew)
            deltaDistance = deltaDistance - hit.distance;
            hit = Physics2D.CircleCast(hit.centroid + velocity * 0.001f, chapaRadius, velocity, deltaDistance);
            if (hit.collider != null)
            {
                Debug.Log("multiple bounces!");
            }*/
        }

        transform.position = position;
    }

    void Beat()
    {
        if (alwaysPressed || Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lastDirection = (mousePos - position).normalized;

            if (invertDirection)
            {
                lastDirection *= -1f;
            }

            if (accelerationTime == 0f)
            {
                velocity += accelerationAmount * lastDirection;
            }
            remainingTimeOnAir = airTime;
        }

        if (allowBrake && Input.GetMouseButton(1))
        {
            velocity = Vector2.zero;
        }
    }

}
