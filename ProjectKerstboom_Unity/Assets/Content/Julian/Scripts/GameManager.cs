﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private int m_ammountOfRound;
    [SerializeField] private float m_spawnDistance;
    private Weapon m_weapon;

    // Synced variable
    private int m_currentRound = 1;
    private PlayerData[] m_playerData = new PlayerData[0];


    private bool m_gameReady = false;

    private List<PlayerData> m_playersLoadedIn = new List<PlayerData>();

    private Player[] m_playersInRoom;

    private static GameManager Instance;


    private void Awake()
    {
        // Create A singelton of this game manager
        Instance = this;

        // Create a local variable
        m_playersInRoom = PhotonNetwork.PlayerList;

        // Only Run On Functions if this is the master client
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerController.m_onPlayerStarted += OnPlayerStarted;
            PlayerController.m_onPlayerDeath += OnPlayerDeath;
        }
    }

    private void Start()
    {
        m_weapon = FindObjectOfType<Weapon>();
    }

    private void OnDestroy()
    {
        PlayerController.m_onPlayerStarted -= OnPlayerStarted;
        PlayerController.m_onPlayerDeath -= OnPlayerDeath;
    }



    /// <summary>
    /// Returns the Player Data
    /// </summary>
    public static PlayerData[] GetPlayerData
    {
        get
        {
            if (Instance)
            {
                return Instance.m_playerData;
            }
            else
            {
                return new PlayerData[0];
            }
        }
    }

    /// <summary>
    /// Returns the current round
    /// </summary>
    public static int GetCurrentRound
    {
        get
        {
            if (Instance)
            {
                return Instance.m_currentRound;
            }
            else
            {
                return 0;
            }
        }

    }


    private void OnPlayerDeath()
    {
        Debug.Log("OnPlayerDeath Called");

        // Create a list of the surviving players
        List<int> playersAliveIndex = new List<int>();

        // Loop and add them to a list
        for (int i = 0; i < m_playerData.Length; i++)
        {
            if (m_playerData[i].m_playerController.IsAlive)
            {
                playersAliveIndex.Add(i);
            }
        }

        // Check if there is only one person alive
        if (playersAliveIndex.Count == 1)
        {
            // So yes end the round
            OnRoundEnd(playersAliveIndex[0]);
        }
    }

    private void OnRoundEnd(int playerDataIndex)
    {
        // Add a point to the score
        m_playerData[playerDataIndex].score++;

        LoadNextRound();
    }

    private void LoadNextRound()
    {
        // Count the rounds up
        m_currentRound++;

        if (m_weapon != null)
        {
            // Reset the weapon
            m_weapon.ResetWeapon();
        }

        // Load nex round for all clients
        photonView.RPC("LoadNextRoundRPC", RpcTarget.All);
    }
    [PunRPC] 
    private void LoadNextRoundRPC()
    {
        for (int i = 0; i < m_playerData.Length; i++)
        {
            /* Spawn Players in a circle */

            // Distance around the circle 
            float radians = 2 * Mathf.PI / m_playerData.Length * i;

            // Get the vector direction
            Vector3 spawnDirection = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));

            // Get the spawn position, center + direction * distance (how far away from the center)
            Vector3 spawnPosition = transform.position + spawnDirection * m_spawnDistance;


            // Apply the spawn position
            m_playerData[i].m_playerController.transform.position = spawnPosition;


            // Reset the player
            m_playerData[i].m_playerController.ResetToDefalt();
        }
    }



    private void OnPlayerStarted(PlayerController m_playerController)
    {

        // Create a new player data
        PlayerData playerData = new PlayerData();

        playerData.score = 0;
        playerData.m_playerController = m_playerController;

        // Add the player data to the list
        m_playersLoadedIn.Add(playerData);

        // Check if there is a same ammount of players in the game as there sould be based on the room.
        if (m_playersInRoom.Length == m_playersLoadedIn.Count)
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

        // Set the player data array
        m_playerData = m_playersLoadedIn.ToArray();

        // Close the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        m_gameReady = true;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the player data count
            stream.SendNext(m_playerData.Length);

            for (int i = 0; i < m_playerData.Length; i++)
            {
                // Send the player controller view id
                stream.SendNext(m_playerData[i].m_playerController.photonView.ViewID);

                // Send the player data score
                stream.SendNext(m_playerData[i].score);
            }


            // send the currend round
            stream.SendNext(m_currentRound);
        }
        else
        {
            int playerDataCount = (int)stream.ReceiveNext();

            List<PlayerData> playerDataList = new List<PlayerData>();
            for (int i = 0; i < playerDataCount; i++)
            {
                PlayerData playerData = new PlayerData();

                // Get the player controller
                playerData.m_playerController =  PhotonNetwork.GetPhotonView((int)stream.ReceiveNext()).GetComponent<PlayerController>();

                // Get the score 
                playerData.score = (int)stream.ReceiveNext();

                // Add the player data back
                playerDataList.Add(playerData);
            }

            // Replace the player data array
            m_playerData = playerDataList.ToArray();

            m_currentRound = (int)stream.ReceiveNext();
        }
    }


    public struct PlayerData
    {
        public int score;
        public PlayerController m_playerController;
    }
}


