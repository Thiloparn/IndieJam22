using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

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
    public static float errorMargin = 0.2f;

    private static Animator chapaAnim;

    public ChapaScript chapaScript;
    // temp variables, for debugging
    public Color backgroundMainColor;
    public Color backgroundBeatColor;

    // FMOD
    private FMOD.Studio.EventInstance songInstance;
    private FMOD.Studio.EVENT_CALLBACK cb;

    // N�mero de beats que llevamos
    static int beatCounter;

    // Tiempo total transcurrido
    static float timePassed;

    //El supuesto tiempo que hay entre un beat y el siguiente
    static float beatTime;

    bool paused;

    // Beat actual y el �ltimo
    static Beat lastBeat;


    // Estamos en la parte del beat de después del clock exacto
    static bool afterBeat;
    static float afterBeatCounter;

    // El tempo de la canción que está sonando
    static int songTempo;


    private void Awake()
    {
        // Callback para los beats
        cb = new FMOD.Studio.EVENT_CALLBACK(BeatCallback);
        string cancionStr = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Cargamos la canción y le ponemos el callback
        songInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Song_" + cancionStr);
        songInstance.setCallback(cb, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
    }

    // Start is called before the first frame update
    void Start()
    {
        chapaAnim = chapaScript.GetComponent<Animator>();
        chapaAnim.speed = 2f;
        //animSt.speed = 2f;

        // Variables de control
        beatCounter = 0;
        beatTime = 0;
        timePassed = 0;
        paused = false;

        afterBeat = false;
        afterBeatCounter = 0;

        // Beat
        lastBeat.no = 0;
        lastBeat.instant = 0;
        lastBeat.click = ClickType.Unused;

        // Empezamos a reproducir la canción
        songInstance.start();
        Debug.Log("Song start");
    }

    // Update is called once per frame
    void Update()
    {
        // Tiempo para que acabe el beat
        if (afterBeat) 
        {
            afterBeatCounter += Time.deltaTime;
            if (afterBeatCounter > beatTime * errorMargin) 
            {
                afterBeat = false;
                afterBeatCounter = 0;
                EndBeat();
            }   
        }

        // Hacemos click...
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Clicked in " + timePassed);
            // Solo nos interesa si es la primera vez que hacemos click en este beat
            if (lastBeat.click == ClickType.Unused)
            {
                float diff = Mathf.Abs(timePassed - lastBeat.instant);
                float timeMargin = beatTime * errorMargin;

                //Debug.Log(timeMargin);

                // Estamos dentro del margen del Beat -> ACIERTO
                if (diff < (timeMargin) ||
                    diff > (beatTime - timeMargin))
                {
                    //float accuracy = 0f;
                    //if (diff < timeMargin)
                    //    accuracy = Accuracy(diff, beatTime);
                    //else
                    //    accuracy = Accuracy(beatTime - diff, beatTime);
                    // Actualizamos el n� de beat
                    lastBeat.click = ClickType.Hit;
                    lastBeat.no = beatCounter;
                    BeatHit();
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
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;

            Time.timeScale = paused ? 0f : 1f;
            songInstance.setPaused(paused);

            Debug.Log(paused ? "Pause" : "Resume");
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
        //Debug.Log("Beat " + beatCounter + " HIT (" + (int)(accuracy * 100f) + "%)");
    }

    private void BeatMiss()
    {
        chapaScript.BeatMiss();
        //Debug.Log("Beat " + beatCounter + " MISS");
    }


    // Mamma mia
    delegate void BeatCallbackDelegate(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters);


    [MonoPInvokeCallback(typeof(BeatCallbackDelegate))]
    static public FMOD.RESULT BeatCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters)
    {
        // Para marcadores que haya puesto en el timeline
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            FMOD.Studio.TIMELINE_MARKER_PROPERTIES marker = 
                (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));

            //Debug.Log((string)marker.name);
           
            
            //int nameLen = 0;
            //while (Marshal.ReadByte(namePtr, nameLen) != 0) ++nameLen;
            //byte[] buffer = new byte[nameLen];
            //Marshal.Copy(namePtr, buffer, 0, buffer.Length);
            //string name = Encoding.UTF8.GetString(buffer, 0, nameLen);
            //if (name == "HIGH")
            //{
            //    UnityEngine.Debug.Log("Reached high intensity marker");
            //}
        }

        // Estamos en un beat de la canción
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            //Debug.Log("Beat " + beatCounter + " en " + timePassed + ", " + beatTime + "s para el siguiente");
            beatCounter++;
            afterBeat = true;


            FMOD.Studio.TIMELINE_BEAT_PROPERTIES beat = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
            songTempo = (int)beat.tempo;

            chapaAnim.speed = songTempo / 120f;

            // Actualizar los contadores
            beatTime = timePassed - lastBeat.instant;
            lastBeat.instant = timePassed;
        }

        //Invoke("EndBeat", beatTime * errorMargin);
        return FMOD.RESULT.OK;
    }


    // Acaba el beat
    private void EndBeat()
    {
        //Debug.Log("Beat ended in " + timePassed);
        //arrowAnimation.speed = beatTime;

        // Se ha terminado el beat y no hemos clicado
        if (lastBeat.no < beatCounter && lastBeat.click == ClickType.Unused)
        {
            //Miss
            lastBeat.click = ClickType.Miss;
            lastBeat.no = beatCounter;
            BeatMiss();
        }
        lastBeat.click = ClickType.Unused;
    }


    private void OnDestroy()
    {
        // Paramos la canción, le quitamos el callback y la liberamos (orden inverso al de la creación)
        Debug.Log("Destroy");
        songInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); //INMEDIATE
        songInstance.setCallback(null);
        songInstance.release();

        Debug.Log("Destroy end");
    }
}
