using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int m_ammountOfRound;

    private int m_currentRound = 1;
    private List<PlayerController> m_playersPlaying = new List<PlayerController>();
    private Dictionary<string,int> m_playerScores = new Dictionary<string, int>();

    public static System.Action<int> m_onRoundChange;
    public static System.Action<Dictionary<string, int>> m_onGameStart;
    public static System.Action<Dictionary<string, int>> m_onPlayerScoreChange;


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

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }

    private void AddPlayer(PlayerController playerController)
    {
        m_playersPlaying.Add(playerController);
    }

    private void StartGame()
    {
        photonView.RPC("StartGameRPC", RpcTarget.All);
    }

    [PunRPC]
    private void StartGameRPC()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            m_playerScores.Add(players[i].UserId, 0);
        }

        m_onGameStart?.Invoke(m_playerScores);
    }

    // Only Called on the master
    private void OnPlayerDeath()
    {
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
        {
            OnPlayerWonRound(m_playersAlive[0].GetUserID());
        }
    }


    private void OnPlayerWonRound(string winnerUserId)
    {
        photonView.RPC("OnPlayerWonRoundRPC", RpcTarget.All, winnerUserId);

        // Load the next round after adding the score
        LoadNextRound();
    }
    [PunRPC]
    private void OnPlayerWonRoundRPC(string winnerUserId)
    {
        // Add a point to the player
        m_playerScores[winnerUserId]++;

        // Invoke the player score change action
        m_onPlayerScoreChange?.Invoke(m_playerScores);
    }

    private void LoadNextRound()
    {
        m_currentRound++;

        photonView.RPC("LoadNextRoundRPC", RpcTarget.All, m_currentRound);
    }

    [PunRPC]
    private void LoadNextRoundRPC(int currentRound)
    {
        m_currentRound = currentRound;
        m_onRoundChange?.Invoke(m_currentRound);

        // Respawn Players
        for (int i = 0; i < m_playersPlaying.Count; i++)
            m_playersPlaying[i].ReSpawn();
    }

}
