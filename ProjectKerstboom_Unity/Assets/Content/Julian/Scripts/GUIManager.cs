using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using DG.Tweening;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_roundTextGUI;
    [SerializeField] private TextMeshProUGUI m_playerScoresTextGUI;
    [SerializeField] private TextMeshProUGUI m_countdownTextGUI;
    [SerializeField] private GameObject m_InGameUI;
    [SerializeField] private GameObject m_GameEndUI;


    private void Awake()
    {
        m_roundTextGUI.gameObject.SetActive(true);
        m_playerScoresTextGUI.gameObject.SetActive(true);
        m_countdownTextGUI.gameObject.SetActive(false);

        m_InGameUI.SetActive(true);
        m_GameEndUI.SetActive(false);
    }


    private void OnEnable()
    {
        GameManager.m_onRoundCountdown += OnRoundCountdown;
        GameManager.m_onGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        GameManager.m_onRoundCountdown -= OnRoundCountdown;
        GameManager.m_onGameEnd -= OnGameEnd;

    }

    private void LateUpdate()
    {
        UpdatePlayerScoresText();
        UpdateRoundText();
    }



    private void OnRoundCountdown(int secondsDelay)
    {
        StartCoroutine(OnRoundCountdownEnumerator(secondsDelay));
    }

    private IEnumerator OnRoundCountdownEnumerator(int secondsDelay)
    {
        m_countdownTextGUI.gameObject.SetActive(true);

        for (int i = secondsDelay; i > 0; i--)
        {
            m_countdownTextGUI.text = i.ToString();
            m_countdownTextGUI.transform.DOPunchScale(Vector3.one, 1f, 1, 0.2f);

            yield return new WaitForSeconds(1);
        }

        m_countdownTextGUI.gameObject.SetActive(false);
    }

    private void UpdatePlayerScoresText()
    {
        string scores = "";

        PlayerData[] playerData = GameManager.GetPlayerData;

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

    private void OnGameEnd(PlayerData[] playerData)
    {
        m_InGameUI.SetActive(false);
        m_GameEndUI.SetActive(true);
    }

}
