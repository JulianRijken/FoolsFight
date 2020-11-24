using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class HammerMovementTesting : MonoBehaviour
{

    [SerializeField] private Transform m_spawnPointParent;
    [SerializeField] private AnimationCurve m_verticalCurve;
    [SerializeField] private float m_minimalDistance;
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private LayerMask m_blockLayer;

    private List<Vector3> m_points;

    private void Start()
    {
        m_points = new List<Vector3>();
        for (int i = 0; i < m_spawnPointParent.childCount; i++)
        {
            m_points.Add(m_spawnPointParent.GetChild(i).position);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            List<Vector3> possiblePoints = new List<Vector3>();
            for (int i = 0; i < m_points.Count; i++)
            {
                float distance = Vector2.Distance(m_points[i], transform.position);
                if(distance > m_minimalDistance)
                {
                    if(!Physics.Linecast(transform.position, m_points[i], m_blockLayer))
                    {
                        possiblePoints.Add(m_points[i]);
                    }
                }
            }

            Vector3 toPoint = Vector3.zero;

            if (possiblePoints.Count > 0)
                toPoint = possiblePoints[Random.Range(0, possiblePoints.Count)];
            else
                Debug.LogError("No Points Found");

            StartCoroutine(MoveToPoint(toPoint));
        }
    }


    private IEnumerator MoveToPoint(Vector3 toPoint)
    {
        Vector3 fromPoint = transform.position;
        float time = 0;

        while(time < 1)
        {
            time += Time.deltaTime * m_moveSpeed;

            Vector3 niewPosition = Vector3.Lerp(fromPoint, toPoint, time);
            niewPosition.y = m_verticalCurve.Evaluate(time) * 6;

            transform.position = niewPosition;
        
            yield return new WaitForEndOfFrame();
        }

        transform.position = toPoint;

    }



    private void OnDrawGizmos()
    {
        if (m_spawnPointParent == null)
            return;

        for (int i = 0; i < m_spawnPointParent.childCount; i++)
        {
            Vector3 position = m_spawnPointParent.GetChild(i).position;
            Gizmos.DrawSphere(position, 0.2f);
        }
    }
}
