using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DirectorManager : MonoBehaviour
{

    [SerializeField] private PlayableDirector m_gameEndDirector;

    public static Action m_onHidePlayers;

    private void OnEnable()
    {
        GameManager.m_onGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        GameManager.m_onGameEnd -= OnGameEnd;
    }


    private void OnGameEnd(PlayerData[] playerData)
    {
        m_gameEndDirector.Play();
    }

    public void SetPlayersBackToRoom()
    {
        // Check if the player sending everyone back is the master
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(0);
    }

    public void HideGameplay()
    {
        // Disable the player controllers
        PlayerController[] controllers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController controller in controllers)
        {
            controller.gameObject.SetActive(false);
        }

        // Disable all the weapons
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }
    }
}
