using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int m_ammountOfRound;

    private int m_currentRound = 1;

    private List<PlayerData> m_playersInGame = new List<PlayerData>();
    private Player[] m_playersInRoom;

    public static System.Action<int> m_onRoundChange;
    public static System.Action<Dictionary<string, int>> m_onGameStart;
    public static System.Action<Dictionary<string, int>> m_onPlayerScoreChange;


    private void Awake()
    {
        // Only Run On Functions if this is the master client
        if(PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            PlayerController.m_onPlayerStarted += OnPlayerStarted;

            // Create a local variable
            m_playersInRoom = PhotonNetwork.PlayerList;
        }
    }



    private void OnDestroy()
    {
        PlayerController.m_onPlayerStarted -= OnPlayerStarted;
    }



    private void OnPlayerStarted(PlayerController m_playerController)
    {
        // Create a new player data
        PlayerData playerData = new PlayerData();

        playerData.score = 0;
        playerData.m_playerController = m_playerController;

        // Add the player data to the list
        m_playersInGame.Add(playerData);

        // Check if there is a same ammount of players in the game as there sould be based on the room.
        if(m_playersInRoom.Length == m_playersInGame.Count)
        {
            OnGameReady();
        }
        else
        {
            Debug.Log("Waiting for more players");
        }
    }

    private void OnGameReady()
    {
        Debug.Log("On Game Ready");

        // Close the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }


    private struct PlayerData
    {
        public int score;
        public PlayerController m_playerController;
    }

}
