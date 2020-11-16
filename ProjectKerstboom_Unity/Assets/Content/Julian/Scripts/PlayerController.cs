using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float m_walkSpeed;
    [SerializeField] private float m_movementLerpSpeed;
    [SerializeField] private float m_rotationSlerpSpeed;

    private Rigidbody m_rigidbody;
    private Vector2 m_movementInput;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 movement = Vector2.zero;
        movement.x = m_movementInput.normalized.x;
        movement.y = 0f;
        movement.z = m_movementInput.normalized.y;

        Vector3 finalMovement = movement * m_walkSpeed;
        m_rigidbody.velocity = Vector3.MoveTowards(m_rigidbody.velocity, finalMovement,Time.deltaTime * m_movementLerpSpeed);
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        m_movementInput = context.ReadValue<Vector2>();
    }
}
