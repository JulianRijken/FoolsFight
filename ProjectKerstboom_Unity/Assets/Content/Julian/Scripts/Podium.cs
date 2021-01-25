using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Podium : MonoBehaviour
{

    [SerializeField] private SkinnedMeshRenderer[] m_skinnedMeshRenderers;
    [SerializeField] private TextMeshPro[] m_playerNames;


    private void Awake()
    {
        GameManager.m_onGameEnd += OnGameEnd;
    }
    private void OnDestroy()
    {
        GameManager.m_onGameEnd -= OnGameEnd;

    }

    private void OnGameEnd(PlayerData[] playerData)
    {
        SetupPodium(playerData);
    }

    private void SetupPodium(PlayerData[] playerData)
    {

        List<PlayerData> sortedPlayers = SortPlayers(playerData);


        for (int i = 0; i < m_skinnedMeshRenderers.Length; i++)
        {
            if(sortedPlayers.Count > i)
            {
                // Set the skin
                SkinnedMeshRenderer skinnedMeshRenderer = sortedPlayers[i].m_playerController.PlayerMeshRenderer;
                m_skinnedMeshRenderers[i].material = skinnedMeshRenderer.material;
                m_skinnedMeshRenderers[i].sharedMesh = skinnedMeshRenderer.sharedMesh;

                // Set the name
                m_playerNames[i].text = sortedPlayers[i].m_playerController.photonView.Owner.NickName;
            }
            else
            {
                // Disable if there are not enough players
                m_skinnedMeshRenderers[i].enabled = false;
                m_playerNames[i].enabled = false;
            }
        }
    }

    private List<PlayerData> SortPlayers(PlayerData[] originalPlayerData)
    {

        // Create the lists
        List<PlayerData> playersToPickFrom = new List<PlayerData>();
        List<PlayerData> sortedPlayers = new List<PlayerData>();

        // Add all the players to the players to pick from list
        for (int i = 0; i < originalPlayerData.Length; i++)
        {
            playersToPickFrom.Add(originalPlayerData[i]);
        }


        // Quick check if it's only one player
        if (originalPlayerData.Length == 1)
        {
            return playersToPickFrom;
        }


        // Bubble sort the player list into the sorted players list
        while (playersToPickFrom.Count > 0)
        {
            int bestPlayerIndex = 0;

            // loop all the players left to pick from and get the highest one
            for (int i = 1; i < playersToPickFrom.Count; i++)
            {
                if (playersToPickFrom[i].score >= playersToPickFrom[bestPlayerIndex].score)
                {
                    bestPlayerIndex = i;
                }
            }

            // add and remove the highest player
            sortedPlayers.Add(playersToPickFrom[bestPlayerIndex]);
            playersToPickFrom.RemoveAt(bestPlayerIndex);
        }

        // Return the final list
        return sortedPlayers;
    }


}
