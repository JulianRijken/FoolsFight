using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private int m_ammountOfRound;
    [SerializeField] private float m_spawnDistance;
    [SerializeField] private int m_normalRoundStartDelay;
    [SerializeField] private int m_firstRoundStartDelay;

    // Synced variable
    private int m_currentRound = 1;
    private PlayerData[] m_playerData = new PlayerData[0];

    private bool m_gameReady = false;
    private Weapon m_weapon;
    private List<PlayerData> m_playersLoadedIn = new List<PlayerData>();
    private Player[] m_playersInRoom;
    private static GameManager Instance;


    public static Action<int> m_onRoundCountdown;
    public static Action m_onLoadNewRound;


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


    // Only Master
    private void OnPlayerDeath(PlayerController playerController)
    {
        // Make sure the game is ready before doing any game logic with the player
        if (!m_gameReady)
            return;

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

    // Only Master
    private void OnRoundEnd(int playerDataIndex)
    {
        // Add a point to the score
        m_playerData[playerDataIndex].score++;

        LoadNewRound(m_currentRound + 1);
    }

    private void LoadNewRound(int round)
    {
        // Count the rounds up
        m_currentRound = round;

        // Reset the weapon
        m_weapon.ResetWeapon();


        // Get a list of all the spawn positions
        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < m_playerData.Length; i++)
        {
            // Distance around the circle 
            float radians = 2 * Mathf.PI / m_playerData.Length * i;

            // Get the vector direction
            Vector3 spawnDirection = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));

            // Add the spawn position, center + direction * distance (how far away from the center)
            spawnPositions.Add(transform.position + spawnDirection * m_spawnDistance);
        }


        // Randomize spawn points, Credits to Matt Howells
        System.Random random = new System.Random();
        int n = spawnPositions.Count;
        while (n > 1)
        {
            int k = random.Next(n--);
            Vector3 temp = spawnPositions[n];
            spawnPositions[n] = spawnPositions[k];
            spawnPositions[k] = temp;
        }


        // Load nex round for all clients
        photonView.RPC("LoadNewRoundRPC", RpcTarget.All, spawnPositions.ToArray(),round);
    }
    [PunRPC] 
    private void LoadNewRoundRPC(Vector3[] spawnPositions, int round)
    {

        Debug.Log("Load New Round RPC Called");
        Debug.Log("Round Countdown player data: " + m_playerData.Length);

        Debug.LogWarning("Probeer het probleem met een docent op te lossen dat de data niet verzonden kan worden en dat de game te snel start");


        m_onLoadNewRound?.Invoke();

        // Set all the players to the correct positions etc
        for (int i = 0; i < m_playerData.Length; i++)
        {
            // Make sure all players are inactive
            m_playerData[i].m_playerController.SetPlayerState(PlayerController.PlayerState.InActive);

            // Apply the spawn position
            m_playerData[i].m_playerController.transform.position = spawnPositions[i];

            Debug.Log("Player Controller Moved");
        }

        // Start the countdown to start the round, also set the delay based on what round it is
        StartCoroutine(RoundCountdown(round == 1 ? m_firstRoundStartDelay : m_normalRoundStartDelay));
    }


    private IEnumerator RoundCountdown(int countdownTime)
    {
        m_onRoundCountdown?.Invoke(countdownTime);
        yield return new WaitForSeconds(countdownTime);

        Debug.Log("Round Countdown player data: " + m_playerData.Length);

        for (int i = 0; i < m_playerData.Length; i++)
        {
            // Set every player to active
            m_playerData[i].m_playerController.SetPlayerState(PlayerController.PlayerState.Walking);

        }
    }




    // Only Master
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

    // Only Master
    private void OnGameReady()
    {
        Debug.Log("On Game Ready");

        // Set the player data array
        m_playerData = m_playersLoadedIn.ToArray();

        // Close the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        
        // Spawn The weapon
        GameObject weaponGameObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Weapon_Hammer"), Vector3.zero, Quaternion.identity);
        m_weapon = weaponGameObject.GetComponent<Weapon>();

        LoadNewRound(1);

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
                playerData.m_playerController = PhotonNetwork.GetPhotonView((int)stream.ReceiveNext()).GetComponent<PlayerController>();

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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player Left The Room");

        // Remove the player from the player data
        for (int i = 0; i < m_playerData.Length; i++)
        {
            if(m_playerData[i].m_playerController.photonView.Owner == otherPlayer)
            {
                List<PlayerData> playerData = m_playerData.ToList();
                playerData.RemoveAt(i);
                m_playerData = playerData.ToArray();
                break;
            }         
        }

        m_playersInRoom = PhotonNetwork.PlayerList;

        if (m_playersInRoom.Length < 2)
            PhotonNetwork.LeaveRoom();
    }


    public struct PlayerData
    {
        public int score;
        public PlayerController m_playerController;
    }
}


