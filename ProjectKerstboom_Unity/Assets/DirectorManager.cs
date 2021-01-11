using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DirectorManager : MonoBehaviour
{

    [SerializeField] private PlayableDirector m_gameEndDirector;

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
        m_gameEndDirector.Play();
    }
}
