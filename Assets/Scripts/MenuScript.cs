using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public GameObject mainMenuScreen;

    public bool onMainMenu = false;
    int selectedChapa = 0;

    public GameObject chapa1;
    public GameObject chapa2;
    public GameObject chapa3;

    private string escena;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (onMainMenu)
        {
            if (Input.anyKeyDown)
            {
                onMainMenu = false;
                mainMenuScreen.SetActive(false);
            }
        }
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
                escena = "Cyberchapas";
            else if (mouseX < .66f)
                escena = "Bouncer";
            else
                escena = "Machine";

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene(escena);
            }
        }
    }
}
