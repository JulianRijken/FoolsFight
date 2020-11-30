using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int m_ammountOfRound;
    private int m_currentRound = 0;

    private List<PlayerController> m_playersPlaying = new List<PlayerController>();

    private void Awake()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PlayerController.m_onPlayerDeath += OnPlayerDeath;
            PlayerController.m_onPlayerStarted += AddPlayer;
        }
    }


    private void OnDestroy()
    {
        PlayerController.m_onPlayerDeath -= OnPlayerDeath;
        PlayerController.m_onPlayerStarted -= AddPlayer;
    }

    private void AddPlayer(PlayerController playerController)
    {
        m_playersPlaying.Add(playerController);
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player Dead");

        List<PlayerController> m_playersAlive = new List<PlayerController>();

        for (int i = 0; i < m_playersPlaying.Count; i++)
        {
            // If a player leaves remove him from the list
            if (m_playersPlaying[i] == null)
                m_playersPlaying.RemoveAt(i);

            if (m_playersPlaying[i].IsAlive())
                m_playersAlive.Add(m_playersPlaying[i]);
        }

        if (m_playersAlive.Count <= 1)
            NextRound();
    }

    private void NextRound()
    {
        Debug.Log("NextRound");
        Debug.Log("players playing " + m_playersPlaying.Count);

        for (int i = 0; i < m_playersPlaying.Count; i++)
        {
            m_playersPlaying[i].ReSpawn();
        }
    }
}
