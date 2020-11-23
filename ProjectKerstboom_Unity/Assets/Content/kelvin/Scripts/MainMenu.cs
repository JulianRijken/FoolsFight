using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Main canvas")]
    [SerializeField] GameObject mainCanvas, optionCanvas, hostCanvas, findRoomCanvas, RoomCanvas;
    public TMP_InputField NameInput;

    [Header("host room")]
    public TMP_InputField roomNameInputField;



    public void HostGame()
    {
        if (string.IsNullOrEmpty(NameInput.text))
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
        }
        else
        {
            PhotonNetwork.NickName = NameInput.text;
        }
        mainCanvas.SetActive(false);
        hostCanvas.SetActive(true);
    }
   
    public void JoinGame()
    {
        if (string.IsNullOrEmpty(NameInput.text))
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
        }
        else
        {
            PhotonNetwork.NickName = NameInput.text;
        }
        mainCanvas.SetActive(false);
        findRoomCanvas.SetActive(true);
    }

    public void Options()
    {
        mainCanvas.SetActive(false);
        optionCanvas.SetActive(true);
    }

    public void Quit()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void Startgame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void BackToMain()
    {
        hostCanvas.SetActive(false);
        optionCanvas.SetActive(false);
        findRoomCanvas.SetActive(false);
        RoomCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        PhotonNetwork.Disconnect();
    }
}
