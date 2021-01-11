using Cinemachine;
using System;
using UnityEngine;

public class TargetGroup : MonoBehaviour
{
    private CinemachineTargetGroup m_cinemachineTargetGroup;


    private void Awake()
    {
        m_cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
        m_cinemachineTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
    }


    private void OnEnable()
    {
        PlayerController.m_onPlayerStarted += OnPlayerJoined;
        PlayerController.m_onPlayerDeath += OnPlayerDied;
        PlayerController.m_onPlayerDestroyed += OnPlayerDestroyed;
        GameManager.m_onLoadNewRound += OnLoadNewRound;
    }

    private void OnDisable()
    {
        PlayerController.m_onPlayerStarted -= OnPlayerJoined;
        PlayerController.m_onPlayerDeath -= OnPlayerDied;
        PlayerController.m_onPlayerDestroyed -= OnPlayerDestroyed;
        GameManager.m_onLoadNewRound -= OnLoadNewRound;
    }

    private void OnPlayerJoined(PlayerController player)
    {
        m_cinemachineTargetGroup.AddMember(player.transform, 1, 1);
    }

    private void OnPlayerDied(PlayerController player)
    {
        int member = m_cinemachineTargetGroup.FindMember(player.transform);
        m_cinemachineTargetGroup.m_Targets[member].weight = 0.0f;
    }

    private void OnPlayerDestroyed(PlayerController player)
    {
        m_cinemachineTargetGroup.RemoveMember(player.transform);
    }
    private void OnLoadNewRound()
    {
        for (int i = 0; i < m_cinemachineTargetGroup.m_Targets.Length; i++)
        {
            m_cinemachineTargetGroup.m_Targets[i].weight = 1.0f;
        }
    }


}
