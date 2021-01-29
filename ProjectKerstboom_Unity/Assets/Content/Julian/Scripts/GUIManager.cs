using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private GameObject m_InGameUI;
    [SerializeField] private GameObject m_GameEndUI;
    [SerializeField] private GameObject m_PauseMenuUI;

    [Header("Rounds")]
    [SerializeField] private TextMeshProUGUI m_roundTextGUI;
    [SerializeField] private TextMeshProUGUI m_countdownTextGUI;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI m_playerScoresTextGUI;

    [Header("Pause Menu")]
    [SerializeField] private Button m_returnToLobbyButton;


    private Controls controls;
    private bool m_isPaused = false;


    private void Awake()
    {
        controls = new Controls();

        m_roundTextGUI.gameObject.SetActive(true);
        m_playerScoresTextGUI.gameObject.SetActive(true);
        m_countdownTextGUI.gameObject.SetActive(false);

        m_InGameUI.SetActive(true);
        m_GameEndUI.SetActive(false);
        m_PauseMenuUI.SetActive(false);
    }

    private void Start()
    {
        // Only allow the master to interact with the button
        m_returnToLobbyButton.interactable = PhotonNetwork.IsMasterClient;     
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.UI.PauseGame.performed += OnPauseGameInput;

        GameManager.m_onRoundCountdown += OnRoundCountdown;
        GameManager.m_onGameEnd += OnGameEnd;

    }

    private void OnDisable()
    {
        controls.Disable();
        controls.UI.PauseGame.performed -= OnPauseGameInput;

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

            scores += $"{nickName} {playerData[i].score}\n";
        }

        m_playerScoresTextGUI.text = scores;
    }

    private void UpdateRoundText()
    {
        m_roundTextGUI.text = $"{GameManager.GetCurrentRound}";
    }

    private void OnGameEnd(PlayerData[] playerData)
    {
        m_InGameUI.SetActive(false);
        m_GameEndUI.SetActive(true);
    }


    #region PauseMenu

    private void OnPauseGameInput(InputAction.CallbackContext context)
    {
        ToggleGamePaused();
    }

    private void ToggleGamePaused()
    {
        m_isPaused = !m_isPaused;
        m_PauseMenuUI.SetActive(m_isPaused);
    }


    public void OnResumeButton()
    {
        ToggleGamePaused();
    }

    public void OnQuitGameButton()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void OnReturnToLobbyButton()
    {
        PhotonNetwork.LoadLevel(0);
    }


    #endregion

}
