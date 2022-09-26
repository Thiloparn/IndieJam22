using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Runtime.InteropServices;
using System.Text;

public enum ClickType { Hit, Miss, Unused }

public struct Beat
{
    // Si se ha acertado
    public ClickType click;

    // N�mero de beat que es
    public int no;

    // Instante (desde que empieza la canci�n) del beat
    public float instant;
}

public class BeatManager : MonoBehaviour
{
    [Tooltip("Selecciona una canci�n para que suene en esta escena")]
    public FMODUnity.EventReference cancion;

    [Tooltip("Porcentaje de error que se le permite al jugador al clicar en el beat")]
    [Range(0f, 1.0f)]
    public float errorMargin = 0.1f;

    public Chapa2Script chapaScript;
    // temp variables, for debugging
    public Color backgroundMainColor;
    public Color backgroundBeatColor;

    // FMOD
    private FMOD.Studio.EventInstance songInstance;
    private FMOD.Studio.EVENT_CALLBACK cb;

    // N�mero de beats que llevamos
    int beatCounter;

    // Tiempo total transcurrido
    float timePassed;

    //El supuesto tiempo que hay entre un beat y el siguiente
    float beatTime;

    bool paused;

    // Beat actual y el �ltimo
    Beat lastBeat;


    private void Awake()
    {
        songInstance = FMODUnity.RuntimeManager.CreateInstance(cancion);

        cb = new FMOD.Studio.EVENT_CALLBACK(BeatCallback);
        songInstance.setCallback(cb, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
        //songInstance.start();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Variables de control
        beatCounter = 0;
        beatTime = 0;
        timePassed = 0;
        paused = false;

        // Beat
        lastBeat.no = 0;
        lastBeat.instant = 0;
        lastBeat.click = ClickType.Unused;
    }

    // Update is called once per frame
    void Update()
    {
        // Hacemos click...
        if (Input.GetMouseButtonDown(0))
        {
            // Solo nos interesa si es la primera vez que hacemos click en este beat
            if (lastBeat.click == ClickType.Unused)
            {
                float diff = Mathf.Abs(timePassed - lastBeat.instant);
                float timeMargin = beatTime * errorMargin;

                // Estamos dentro del margen del Beat -> ACIERTO
                if (diff < (timeMargin) ||
                    diff > (beatTime - timeMargin))
                {
                    float accuracy = 0f;
                    if (diff < timeMargin)
                        accuracy = Accuracy(diff, beatTime);
                    else
                        accuracy = Accuracy(beatTime - diff, beatTime);
                    // Actualizamos el n� de beat
                    lastBeat.click = ClickType.Hit;
                    lastBeat.no = beatCounter;
                    BeatHit(accuracy);
                }

                // No lo estamos -> ERROR
                else
                {
                    lastBeat.click = ClickType.Miss;
                    BeatMiss();
                }
            }
        }

        // Pausar la canci�n
        if (Input.GetKeyDown(KeyCode.P))
        {
            paused = !paused;
            songInstance.setPaused(paused);
            Debug.Log("PAUSED");
        }

        float t = (timePassed - lastBeat.instant) / beatTime;
        Camera.main.backgroundColor = Color.Lerp(backgroundBeatColor, backgroundMainColor, t * t);

        timePassed += Time.deltaTime;
    }

    private float Accuracy(float diff, float beatTime)
    {
        return (diff / (beatTime / 2) - 1) / (-1);
    }

    private void BeatHit(float accuracy = 1)
    {
        chapaScript.BeatHit(accuracy);
        Debug.Log("Beat " + beatCounter + " HIT (" + (int)(accuracy * 100f) + "%)");
    }

    private void BeatMiss()
    {
        chapaScript.BeatMiss();
        Debug.Log("Beat " + beatCounter + " MISS");
    }


    public FMOD.RESULT BeatCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters)
    {
        //Debug.Log("Beat " + beatCounter + " en " + timePassed + ", " + beatTime + "s para el siguiente");

        //if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        //{
        //    FMOD.Studio.TIMELINE_MARKER_PROPERTIES marker = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
        //    IntPtr namePtr = marker.name;
        //    int nameLen = 0;
        //    while (Marshal.ReadByte(namePtr, nameLen) != 0) ++nameLen;
        //    byte[] buffer = new byte[nameLen];
        //    Marshal.Copy(namePtr, buffer, 0, buffer.Length);
        //    string name = Encoding.UTF8.GetString(buffer, 0, nameLen);
        //    if (name == "HIGH")
        //    {
        //        UnityEngine.Debug.Log("Reached high intensity marker");
        //    }
        //}
        //if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        //{
        //    FMOD.Studio.TIMELINE_BEAT_PROPERTIES beat = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
        //}


        // Actualizar los contadores
        beatTime = timePassed - lastBeat.instant;
        lastBeat.instant = timePassed;

        beatCounter++;

        Invoke("EndBeat", beatTime * errorMargin);
        return FMOD.RESULT.OK;
    }

    // Acaba el beat
    private void EndBeat()
    {
        // Se ha terminado el beat y no hemos clicado
        if (lastBeat.no < beatCounter && lastBeat.click == ClickType.Unused)
        {
            lastBeat.click = ClickType.Miss;
            lastBeat.no = beatCounter;
            BeatMiss();
        }
        lastBeat.click = ClickType.Unused;

        //Debug.Log("Beat ended in " + timePassed);
    }


    private void OnDestroy()
    {
        songInstance.setCallback(null);
        songInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); //INMEDIATE
        songInstance.release();
    }
}
