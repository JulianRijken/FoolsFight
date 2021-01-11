﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Podium : MonoBehaviour
{

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
        SetupPodium(playerData);
    }

    private void SetupPodium(PlayerData[] playerData)
    {
        // Add all the players to a list
        List<PlayerData> playersToPickFrom = new List<PlayerData>();
        for (int a = 0; a < playerData.Length; a++)
        {
            playersToPickFrom.Add(playerData[a]);
        }


        // Create a list for all the sorted players
        List<PlayerData> sortedPlayers = new List<PlayerData>();


        // While there are players not sorted sort
        while(playersToPickFrom.Count > 0)
        {
            int playerWithBestScore = 0;

            // loop all the players left to pick from and get the heighest one
            for (int i = 0; i < playersToPickFrom.Count; i++)
            {
                if (playerData[i].score > playerData[playerWithBestScore].score)
                {
                    playerWithBestScore = i;
                }
            }

            // add and remove the heighest player
            sortedPlayers.Add(playerData[playerWithBestScore]);
            playersToPickFrom.RemoveAt(playerWithBestScore);
        }


        // Nu kan je hier custom players laten dansen


    }

}
