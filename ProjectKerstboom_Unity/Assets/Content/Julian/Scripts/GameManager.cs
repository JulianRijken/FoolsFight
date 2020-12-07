using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private int m_ammountOfRound;

    private int m_currentRound = 1;
    private List<PlayerData> m_playerData = new List<PlayerData>();

    private Player[] m_playersInRoom;

    public static Action<int> m_onRoundChange;
    public static Action m_onGameReady;
    public static Action m_onPlayerScoreChange;

    private static GameManager Instance;


    private void Awake()
    {
        if (photonView.IsMine)
        {
            // Create A singelton of this game manager
            Instance = this;

            // Only Run On Functions if this is the master client
            if (PhotonNetwork.IsMasterClient)
            {
                PlayerController.m_onPlayerStarted += OnPlayerStarted;

                // Create a local variable
                m_playersInRoom = PhotonNetwork.PlayerList;
            }
        }
    }



    private void OnDestroy()
    {
        PlayerController.m_onPlayerStarted -= OnPlayerStarted;
    }

    /// <summary>
    /// Returns the Player Data
    /// </summary>
    public static List<PlayerData> GetPlayerData
    {
        get
        {
            if (Instance)
            {
                return Instance.m_playerData;
            }
            else
            {
                return new List<PlayerData>();
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


    private void OnPlayerStarted(PlayerController m_playerController)
    {

        // Create a new player data
        PlayerData playerData = new PlayerData();

        playerData.score = 0;
        playerData.m_playerController = m_playerController;

        // Add the player data to the list
        m_playerData.Add(playerData);

        // Check if there is a same ammount of players in the game as there sould be based on the room.
        if (m_playersInRoom.Length == m_playerData.Count)
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

        m_onGameReady?.Invoke();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Check if the one sending is the master and the local one
            if(PhotonNetwork.IsMasterClient && photonView.IsMine)
            {
                stream.SendNext(m_playerData);
                stream.SendNext(m_currentRound);
            }
 
        }
        else
        {
            // Make sure the one receving the data is not the master
            if (!PhotonNetwork.IsMasterClient)
            {
                m_playerData = (List<PlayerData>)stream.ReceiveNext();
                m_currentRound = (int)stream.ReceiveNext();
            }
        }
    }

    public struct PlayerData
    {
        public int score;
        public PlayerController m_playerController;
    }
}


