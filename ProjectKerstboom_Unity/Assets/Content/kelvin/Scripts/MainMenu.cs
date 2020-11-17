using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Main canvas")]
    public GameObject mainCanvas, optionCanvas, hostCanvas;
    public TMP_InputField NameInput;


    public void HostGame()
    {
        mainCanvas.SetActive(false);
        hostCanvas.SetActive(true);
    }
   
    public void JoinGame()
    {

    }

    public void Options()
    {
        mainCanvas.SetActive(false);
        optionCanvas.SetActive(true);
    }

    public void Quit()
    {
        EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
