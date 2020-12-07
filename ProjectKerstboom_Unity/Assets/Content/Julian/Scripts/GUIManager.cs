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


    private void LateUpdate()
    {
        UpdatePlayerScoresText();
        UpdateRoundText();
    }

    private void UpdatePlayerScoresText()
    {
        string scores = "";

        GameManager.PlayerData[] playerData = GameManager.GetPlayerData;

        for (int i = 0; i < playerData.Length; i++)
        {
            PhotonView playerPhotonView = playerData[i].m_playerController.photonView;
            string nickName = playerPhotonView.Owner == null ? "Error" : playerPhotonView.Owner.NickName;

            scores += $"Name:{nickName} Score: {playerData[i].score}\n";
        }

        m_playerScoresTextGUI.text = scores;
    }

    private void UpdateRoundText()
    {
        m_roundTextGUI.text = $"Round {GameManager.GetCurrentRound}";
    }

}
