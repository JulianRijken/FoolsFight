using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using TMPro;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Main canvas")]
    [SerializeField] GameObject mainMenu, optionMenu, hostMenu, findRoomMenu, RoomMenu;
    [Header("RectTransform")]
    [SerializeField] RectTransform mainMenuTran, optionMenuTran, hostMenuTran, findRoomMenuTran, RoomMenuTran;
    public TMP_InputField nameInput;

    [Header("host room")]
    public TMP_InputField roomNameInputField;

    private void Start()
    {
        roomNameInputField.characterLimit = 10;
        nameInput.characterLimit = 10;
    }

    public void HostGame()
    {
        
        if (string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
        }
        else
        {
            PhotonNetwork.NickName = nameInput.text;
        }
        mainMenuTran.DOAnchorPos(new Vector2(-1500, 0), 0.50f);
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
        hostMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
    }
   
    public void JoinGame()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
        }
        else
        {
            PhotonNetwork.NickName = nameInput.text;
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
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    public void BackToMain()
    {
        SceneManager.LoadScene(0);
        hostMenu.SetActive(false);
        optionMenu.SetActive(false);
        findRoomMenu.SetActive(false);
        mainMenu.SetActive(true);
        mainMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
    }
    public void LeaveRoom()
    {
        SceneManager.LoadScene(0);
        RoomMenuTran.DOAnchorPos(new Vector2(0, -1500), 0.50f);
        RoomMenu.SetActive(false);
        mainMenu.SetActive(true);
        mainMenuTran.DOAnchorPos(new Vector2(0, 0), 0.50f);
        PhotonNetwork.LeaveRoom();
    }

}
