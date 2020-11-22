using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorPass : MonoBehaviour
{
    public void OnWeaponUsed() { m_onWeaponUsed?.Invoke(); }
    public static Action m_onWeaponUsed;
}
