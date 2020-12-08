using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayersTest : MonoBehaviour
{
    [SerializeField] private float m_distanceFromPoint;
    [SerializeField] private float m_players;

    private void OnDrawGizmos()
    {

        for (int i = 0; i < m_players; i++)
        {
            // Distance around the circle 
            float radians = 2 * Mathf.PI / m_players * i;

            // Get the vector direction
            Vector3 spawnDirection = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));

            // Get the spawn position, center + direction * distance (how far away from the center)
            Vector3 spawnPosition = transform.position + spawnDirection * m_distanceFromPoint;

            Gizmos.DrawSphere(spawnPosition, 0.5f);
        }

    }
}
