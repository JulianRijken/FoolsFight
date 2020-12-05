using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_roundTextGUI;
    [SerializeField] private TextMeshProUGUI m_playerScoresTextGUI;

    private void Awake()
    {
        GameManager.m_onRoundChange += OnNextRound;
        GameManager.m_onGameStart += OnGameStart;
        GameManager.m_onPlayerScoreChange += OnPlayerScoreChange;
    }

    private void OnDestroy()
    {
        GameManager.m_onRoundChange -= OnNextRound;
        GameManager.m_onGameStart -= OnGameStart;
        GameManager.m_onPlayerScoreChange -= OnPlayerScoreChange;
    }

    private void OnPlayerScoreChange(Dictionary<string, int> playerScores)
    {
        SetPlayerScores(playerScores);
    }

    private void OnGameStart(Dictionary<string, int> playerScores)
    {
        SetRound(0);
        SetPlayerScores(playerScores);
    }

    private void OnNextRound(int round)
    {
        SetRound(round);
    }



    private void SetPlayerScores(Dictionary<string, int> playerScores)
    {
        string scores = "";

        foreach (KeyValuePair<string, int> attachStat in playerScores)
        {
            //Now you can access the key and value both separately from this attachStat as:
            Debug.Log(attachStat.Key);
            Debug.Log(attachStat.Value);

            scores += $"UserId:{attachStat.Key} Score: {attachStat.Value}\n";
        }

        m_playerScoresTextGUI.text = scores;
    }

    private void SetRound(int round)
    {
        m_roundTextGUI.text = $"Round {round}";
    }

}
