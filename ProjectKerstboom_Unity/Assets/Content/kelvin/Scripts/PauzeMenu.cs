using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauzeMenu : MonoBehaviour
{
    [SerializeField] GameObject pauzeMenu ;
    [SerializeField]  RectTransform pauzeMenuTran;
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

      
    }

    public void Quit()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }
    public void BackToLobby()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }
}
