using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public GameObject mainMenuScreen;

    public bool onMainMenu = false;
    int selectedChapa = 0;

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

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Z))
            {
                SceneManager.LoadScene("GameScene");
            }
        }
    }
}
