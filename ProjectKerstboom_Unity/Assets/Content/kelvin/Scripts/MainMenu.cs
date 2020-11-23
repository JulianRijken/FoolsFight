using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using TMPro;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Main canvas")]
    [SerializeField] GameObject mainMenu, optionMenu, hostMenu, findRoomMenu, RoomMenu;
    [Header("RectTransform")]
    [SerializeField] RectTransform mainMenuTran, optionMenuTran, hostMenuTran, findRoomMenuTran, RoomMenuTran;
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
        mainMenuTran.DOAnchorPos(new Vector2(-1500, 0), 0.50f);
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
        hostMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
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
        mainMenuTran.DOAnchorPos(new Vector2(-1500, 0), 0.50f);
        mainMenu.SetActive(false);
        findRoomMenu.SetActive(true);
        findRoomMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
    }

    public void Options()
    {
        mainMenuTran.DOAnchorPos(new Vector2(-1500, 0), 0.50f);
        mainMenu.SetActive(false);
        optionMenu.SetActive(true);
        optionMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
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
        hostMenu.SetActive(false);
        optionMenu.SetActive(false);
        findRoomMenu.SetActive(false);
        RoomMenu.SetActive(false);
        mainMenu.SetActive(true);
        mainMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
    }
    public void LeaveRoom()
    {
        RoomMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
        RoomMenu.SetActive(false);
        mainMenu.SetActive(true);
        mainMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
        PhotonNetwork.LeaveRoom();
    }
}
