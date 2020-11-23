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

        PlayerController.m_onPlayerStarted += OnPlayerJoined;
    }

    private void OnDestroy()
    {
        PlayerController.m_onPlayerStarted -= OnPlayerJoined;
    }

    private void OnPlayerJoined(Transform newPlayer)
    {
        m_cinemachineTargetGroup.AddMember(newPlayer, 1, 1);
    }

}
