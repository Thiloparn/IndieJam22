using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BankLoader : MonoBehaviour
{
    [SerializeField]
    public string sceneName;

    [FMODUnity.BankRef]
    public List<string> banks;

    //public Button ClickToLoadButton, ChangeSceneButton;

    private void Awake()
    {
        LoadBanks();
    }

    public void LoadBanks()
    {
        foreach (string b in banks)
        {
            FMODUnity.RuntimeManager.LoadBank(b, true);
            Debug.Log("Loaded bank " + b);
        }
        /*
            For Chrome / Safari browsers / WebGL.  Reset audio on response to user interaction (LoadBanks is called from a button press), to allow audio to be heard.
        */
        FMODUnity.RuntimeManager.CoreSystem.mixerSuspend();
        FMODUnity.RuntimeManager.CoreSystem.mixerResume();

        StartCoroutine(CheckBanksLoaded());
    }

    IEnumerator CheckBanksLoaded()
    {
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        Debug.Log("Banks Loaded");
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
