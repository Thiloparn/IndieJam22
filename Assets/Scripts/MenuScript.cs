using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public GameObject mainMenuScreen;

    public bool onMainMenu = false;
    

    public GameObject chapa1;
    public GameObject chapa2;
    public GameObject chapa3;

    int selection = 0;
    private string[] escenas = { "Cyberchapas", "Bouncer", "Machine" };

    //SONIDO
    private FMOD.Studio.EventInstance selectInstance;

    private void Awake()
    {
        selectInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Seleccion");
    }

    // Start is called before the first frame update
    void Start()
    {
        chapa1.SetActive(false);
        chapa2.SetActive(false);
        chapa3.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Portada del menú
        if (onMainMenu)
        {
            if (Input.anyKeyDown)
            {
                selectInstance.start();
                onMainMenu = false;
                mainMenuScreen.SetActive(false);
            }
        }

        // Selección de chapas
        else
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                onMainMenu = true;
                mainMenuScreen.SetActive(true);
                return;
            }
            // todo: elegir chapa - input
            // todo: elegir chapa - visual

            float mouseX = Input.mousePosition.x / Screen.width;

            chapa1.SetActive(mouseX < .33f);
            chapa2.SetActive(mouseX >= .33f && mouseX < .66f);
            chapa3.SetActive(mouseX >= .66f);

            if (mouseX < .33f)
                selection = 0;
            else if (mouseX < .66f)
                selection = 1;
            else
                selection = 2;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0))
            {
                selectInstance.start();
                SceneManager.LoadScene(escenas[selection]);
            }
        }
    }
}
