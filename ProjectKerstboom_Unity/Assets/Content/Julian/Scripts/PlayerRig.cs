using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRig : MonoBehaviour
{
    private Animator m_animator;
    private List<Vector3> m_oldLocalPositions = new List<Vector3>();
    private List<Quaternion> m_oldLocalQuaternions = new List<Quaternion>();

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void SetRagdoll(bool ragdoll, Vector3 velocity, float randomRotationForce)
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
            m_rigidbodys[i].angularVelocity = new Vector3(Random.Range(-1.0f,1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * randomRotationForce;
        }

        for (int i = 0; i < m_colliders.Length; i++)
        {
            m_colliders[i].enabled = ragdoll;
        }



        if (m_oldLocalPositions.Count == 0)
        {
            for (int i = 0; i < m_rigidbodys.Length; i++)
            {
                m_oldLocalPositions.Add(m_rigidbodys[i].transform.localPosition);
            }
        }

        if (m_oldLocalQuaternions.Count == 0)
        {
            for (int i = 0; i < m_rigidbodys.Length; i++)
            {
                m_oldLocalQuaternions.Add(m_rigidbodys[i].transform.localRotation);
            }
        }


        // If going out of ragdoll reset the player transforms
        if (!ragdoll)
        {
            for (int i = 0; i < m_rigidbodys.Length; i++)
            {
                m_rigidbodys[i].transform.localPosition = m_oldLocalPositions[i];
                m_rigidbodys[i].transform.localRotation = m_oldLocalQuaternions[i];
            }
        }

    }
}
