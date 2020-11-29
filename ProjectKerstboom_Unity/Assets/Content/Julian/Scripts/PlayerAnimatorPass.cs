using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorPass : MonoBehaviour
{

    [SerializeField] private PhotonView m_playerPhotonView;

    public void OnWeaponUsed() 
    { 
        if(m_playerPhotonView.IsMine)
            m_onWeaponUsed?.Invoke();
    }
    public static Action m_onWeaponUsed;
}
