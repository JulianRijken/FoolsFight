using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_roundTextGUI;
    [SerializeField] private TextMeshProUGUI m_playerScoresTextGUI;

    private void Awake()
    {
        GameManager.m_onRoundChange += OnNextRound;
        GameManager.m_onGameReady += OnGameReady;
        GameManager.m_onPlayerScoreChange += OnPlayerScoreChange;
    }

    private void OnDestroy()
    {
        GameManager.m_onRoundChange -= OnNextRound;
        GameManager.m_onGameReady -= OnGameReady;
        GameManager.m_onPlayerScoreChange -= OnPlayerScoreChange;
    }

    private void OnPlayerScoreChange()
    {
        UpdatePlayerScoresText();
    }

    private void OnGameReady()
    {
        UpdatePlayerScoresText();
        UpdateRoundText();
    }

    private void OnNextRound(int round)
    {
        UpdateRoundText();
    }


    private void UpdatePlayerScoresText()
    {
        Debug.Log("UpdatePlayerScoresText Called");

        string scores = "";

        List<GameManager.PlayerData> playerData = GameManager.GetPlayerData;

        for (int i = 0; i < playerData.Count; i++)
        {
            PhotonView playerPhotonView = playerData[i].m_playerController.photonView;
            string nickName = playerPhotonView.Owner == null ? "Error" : playerPhotonView.Owner.NickName;

            scores += $"Name:{nickName} Score: {playerData[i].score}\n";
        }

        m_playerScoresTextGUI.text = scores;
    }

    private void UpdateRoundText()
    {
        Debug.Log("UpdateRoundText Called");

        m_roundTextGUI.text = $"Round {GameManager.GetCurrentRound}";
    }

}
