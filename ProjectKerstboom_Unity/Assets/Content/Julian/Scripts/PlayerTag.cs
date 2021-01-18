using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerTag : MonoBehaviour
{
    [SerializeField] private PhotonView m_photonView;
    private TextMeshPro m_text;
    private Camera m_mainCamera;

    private void Awake()
    {
        m_mainCamera = Camera.main;
        m_text = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        m_text.text = m_photonView.Owner == null ? "Error" : m_photonView.Owner.NickName;
    }

    private void LateUpdate()
    { 
        transform.rotation = m_mainCamera.transform.rotation;
    }

    public void ShowTag()
    {
        m_text.enabled = true;
    }

    public void HideTag()
    {
        m_text.enabled = false;
    }
}
