using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FloorType { TrackMain, TrackBorder, Hole, Mud, Ice }

public class ChapaScript : MonoBehaviour
{

    [HideInInspector]
    public Vector2 position = Vector2.zero;
    [HideInInspector]
    public Vector2 velocity = Vector2.zero;
    [HideInInspector]
    public FloorType curFloorType;
    private FloorType prevFloorType;

    [Range(0.0f, 500.0f)]
    public float accelerationAmount;

    [Range(0f, 1.0f)]
    [Tooltip("If greater than 0, it will apply the accelerationAmount a lo largo de esta cantidad de segundos.")]
    public float accelerationTime;

    [Range(0.0f, 20.0f)]
    public float trackMainDrag = 7.54f;

    [Range(0.0f, 20.0f)]
    public float trackOutDrag = 20f;

    [Range(0.0f, 10.0f)]
    public float trackMainWidth = 5.5f;

    [Range(0.0f, 50.0f)]
    public float trackOutWidth = 13.5f;

    [Tooltip("If true, act as if mouse was pressed every beat.")]
    public bool alwaysPressed;

    [Tooltip("If true, right mouse button will stop the chapa.")]
    public bool allowBrake;

    [Tooltip("If true, not having a floor will be taken as a hole.")]
    public bool defaultToHole = false;

    [Tooltip("If true, will set acceleration to 0 between beats.")]
    public bool resetAccelerationOnBeat = false;

    [Tooltip("How much velocity is maintained after bouncing.")]
    [Range(0.0f, 2.0f)]
    public float defaultBounciness;

    [Tooltip("If true, the chapa will be pushed by the mouse instead of pulled.")]
    public bool invertDirection = false;

    [Range(0.0f, 2.0f)]
    public float airTime;

    [Range(0.0f, 10.0f)]
    public float airDrag;

    [Tooltip("When falling in a hole, how many turns to go back.")]
    [Range(1, 5)]
    public int holePenalty = 1;

    List<Vector2> previousPositions;
    int maxPositionsStored = 100;

    float remainingTimeOnAir = 0f;
    float timeSinceLastBeat = 0f;

    Vector2 lastDirection = Vector2.zero;
    [HideInInspector]
    public Vector2 mouseDirection = Vector2.zero;

    float chapaRadius;

    LayerMask wallsMask;
    LayerMask floorsMask;

    public TrackScript trackScript;
    public CameraScript cameraScript;
    public BarreraScript barreraScript;


    //SONIDO
    private FMOD.Studio.EventInstance caidaInstance;
    private FMOD.Studio.EventInstance chorlitoInstance;
    private FMOD.Studio.EventInstance reboteInstance;
    private FMOD.Studio.EventInstance choqueInstance;
    private FMOD.Studio.EventInstance barroInstance;
    private FMOD.Studio.EventInstance hieloInstance;
    private FMOD.Studio.EventInstance zapInstance;


    private void Awake()
    {
        // SONIDO
        caidaInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Caida");
        chorlitoInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Chorlito");
        reboteInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Rebote");
        choqueInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Choque");
        barroInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Barro");
        hieloInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hielo");
        zapInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Zap");
    }

    // Start is called before the first frame update
    void Start()
    {
        // dirty hack <- shame on you
        chapaRadius = transform.localScale.x / 2f;

        floorsMask = LayerMask.GetMask("Floors");
        wallsMask = LayerMask.GetMask("Walls");

        previousPositions = new List<Vector2>(maxPositionsStored);

        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            position = Vector2.zero;
            velocity = Vector2.zero;
            remainingTimeOnAir = 0f;
            timeSinceLastBeat = 0f;
            lastDirection = Vector2.zero;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDirection = (mousePos - position).normalized;
        if (invertDirection)
        {
            mouseDirection *= -1f;
        }
        transform.rotation = Quaternion.FromToRotation(Vector3.right, mouseDirection);

        remainingTimeOnAir = Mathf.Max(0f, remainingTimeOnAir - Time.deltaTime);

        timeSinceLastBeat += Time.deltaTime;

        if (accelerationTime > 0f && timeSinceLastBeat < accelerationTime)
        {

            velocity += accelerationAmount * lastDirection * Time.deltaTime / accelerationTime;

        }

        // Para los sonidos del suelo
        if(curFloorType != prevFloorType) 
        {
            if (curFloorType == FloorType.Ice)
                hieloInstance.start();
            else if (curFloorType == FloorType.Mud)
                barroInstance.start();
        }

        prevFloorType = curFloorType;
    }

    void FixedUpdate()
    {
        float curDrag;

        // todo: this could be optimized a ton
        TileScript closestTile = null;
        float closestTileDistanceSq = float.PositiveInfinity;
        float closestTileTime = 0f; // between 0 and 1

        float curTileTime;
        foreach (TileScript curTile in trackScript.tiles)
        {
            Vector3 curPoint = curTile.trackPath.FindNearestPointTo(transform.position, out curTileTime, 100f);
            curPoint.z = transform.position.z;
            float curTileDistanceSq = Vector3.SqrMagnitude(transform.position - curPoint);
            if (curTileDistanceSq < closestTileDistanceSq)
            {
                closestTile = curTile;
                closestTileDistanceSq = curTileDistanceSq;
                closestTileTime = curTileTime;
            }
        }

        float closestTilePercentage = closestTile.trackPath.evenlySpacedPoints.GetPercentageAtNormalizedT(closestTileTime);
        cameraScript.SetPathPos(closestTile.cameraPath.GetPoint(closestTile.cameraPath.evenlySpacedPoints.GetNormalizedTAtPercentage(closestTilePercentage)));
        if (barreraScript != null)
        {
            // barreraScript.SetChapaPos(closestTile, closestTileTime);
            barreraScript.SetChapaPos(closestTile, closestTilePercentage);
        }

        bool fellToHole = false;
        if (closestTileDistanceSq * 4 < trackMainWidth * trackMainWidth)
        {
            curDrag = trackMainDrag;
            // Debug.Log("track");
            curFloorType = FloorType.TrackMain;
        }
        else if (closestTileDistanceSq * 4 < trackOutWidth * trackOutWidth)
        {
            curDrag = trackOutDrag;
            // Debug.Log("mud");
            curFloorType = FloorType.TrackBorder;
        }
        else
        {
            // Debug.Log("out");
            if (defaultToHole)
            {
                fellToHole = true;
                curDrag = 1f;
                curFloorType = FloorType.Hole;
            }
            else
            {
                curDrag = trackOutDrag;
                curFloorType = FloorType.TrackBorder;
            }
        }

        RaycastHit2D[] floorHits = Physics2D.CircleCastAll(position, chapaRadius, Vector2.zero, 0.0f, floorsMask);
        if (floorHits.Length > 0)
        {
            RaycastHit2D floorHit = floorHits[0];
            float lowestZ = floorHit.transform.position.z;
            for (int k = 0; k < floorHits.Length; k++)
            {
                if (floorHits[k].transform.position.z < lowestZ)
                {
                    floorHit = floorHits[k];
                    lowestZ = floorHit.transform.position.z;
                }
            }
            if (floorHit.collider != null)
            {
                FloorScript curFloor = floorHit.collider.GetComponent<FloorScript>();
                if (curFloor != null)
                {
                    if (curFloor.type == FloorType.Hole)
                    {
                        fellToHole = true;
                    }
                    curDrag = curFloor.drag;
                    curFloorType = curFloor.type;
                }
            }
        }

        if (remainingTimeOnAir > 0f)
        {
            curDrag = airDrag;
            fellToHole = false;
        }

        if (fellToHole && previousPositions.Count > 0)
        {
            //SONIDO
            caidaInstance.start();

            int nToRemove = Mathf.Min(holePenalty - 1, previousPositions.Count);
            previousPositions.RemoveRange(previousPositions.Count - nToRemove, nToRemove);
            position = previousPositions[previousPositions.Count - 1];
            transform.position = position;
            velocity = Vector2.zero;
            remainingTimeOnAir = 0f;
            timeSinceLastBeat = 0f;
            lastDirection = Vector2.zero;
            return;
        }



        velocity -= velocity * curDrag * Time.deltaTime;

        // Bounce against walls!
        float deltaDistance = velocity.magnitude * Time.deltaTime;
        // todo: change to the "nonalloc" version
        RaycastHit2D hit = Physics2D.CircleCast(position, chapaRadius, velocity, deltaDistance, wallsMask);

        position += velocity * Time.deltaTime;

        // while (hit.collider != null)
        if (hit.collider != null)
        {
            float bounciness = defaultBounciness;
            WallScript curWall = hit.collider.GetComponent<WallScript>();
            if (curWall != null)
            {
                bounciness = curWall.bounciness;

                //SONIDO
                if (bounciness < 0.6)
                    choqueInstance.start();
                else
                    reboteInstance.start();
            }
            velocity = Vector2.Reflect(velocity, hit.normal) * bounciness;
            if (accelerationTime > 0f && timeSinceLastBeat < accelerationTime)
            {
                lastDirection = Vector2.Reflect(lastDirection, hit.normal);
            }
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

    public void BeatHit(float accuracy)
    {
        if (barreraScript != null && barreraScript.lose.losing)
        {
            return;
        }

        if (previousPositions.Count >= maxPositionsStored)
        {
            previousPositions.RemoveAt(0);
        }
        previousPositions.Add(position + Vector2.zero);

        timeSinceLastBeat = 0f;

        //SONIDO
        chorlitoInstance.start();
        // Suelo que ralentiza
        if (curFloorType == FloorType.Mud || curFloorType == FloorType.TrackBorder)
            barroInstance.start();
        //else if (curFloorType == FloorType.Ice)
        //TODO


        if (resetAccelerationOnBeat)
        {
            velocity = Vector2.zero;
        }
        lastDirection = mouseDirection;
        if (accelerationTime == 0f)
        {
            velocity += accelerationAmount * lastDirection;
        }
        remainingTimeOnAir = airTime;
    }

    public void BeatMiss()
    {
        if (allowBrake)
        {
            velocity = Vector2.zero;
        }

        if (barreraScript != null)
            barreraScript.BeatMiss();
    }

}
