using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauzeMenu : MonoBehaviour
{
    [SerializeField] GameObject pauzeMenu ;
    [SerializeField]  RectTransform pauzeMenuTran;
    [SerializeField] GameObject backRoomBut;

    public static bool inRoom;

    public bool IsPaused;
    Keyboard keyBoard;
    // Start is called before the first frame update
    void Start()
    {
        keyBoard = Keyboard.current;
        pauzeMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (keyBoard.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                pauzeMenu.SetActive(false);
                IsPaused = false;
            }
            else
            {
                pauzeMenu.SetActive(true);
                IsPaused = true;
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            backRoomBut.SetActive(true);
        }
      
    }

    public void Quit()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }
    public void BackToLobby()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
    
    public void BackToRoom()
    {
        inRoom = true;
        PhotonNetwork.LoadLevel(0);
    }
}
