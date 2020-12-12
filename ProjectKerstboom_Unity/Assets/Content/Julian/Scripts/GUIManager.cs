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


    private void Awake()
    {
        GameManager.m_onRoundCountdown += OnRoundCountdown;

        m_countdownTextGUI.gameObject.SetActive(false);
        m_playerScoresTextGUI.gameObject.SetActive(true);
        m_countdownTextGUI.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        GameManager.m_onRoundCountdown -= OnRoundCountdown;
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

    // Can also be done with a animation
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
