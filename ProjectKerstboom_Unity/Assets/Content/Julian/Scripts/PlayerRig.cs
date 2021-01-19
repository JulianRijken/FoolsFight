using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRig : MonoBehaviour
{
    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void SetRagdoll(bool ragdoll, Vector3 velocity)
    {
        if(m_animator != null)
        {
            m_animator.enabled = !ragdoll;
        }


        Rigidbody[] m_rigidbodys = transform.GetComponentsInChildren<Rigidbody>();
        Collider[] m_colliders = transform.GetComponentsInChildren<Collider>();


        for (int i = 0; i < m_rigidbodys.Length; i++)
        {
            m_rigidbodys[i].isKinematic = !ragdoll;
            m_rigidbodys[i].velocity = velocity;
        }

        for (int i = 0; i < m_colliders.Length; i++)
        {
            m_colliders[i].enabled = ragdoll;
        }

    }
}
