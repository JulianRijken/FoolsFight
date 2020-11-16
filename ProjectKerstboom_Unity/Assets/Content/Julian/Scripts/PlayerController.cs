using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float m_maxMovementSpeed;
    [SerializeField] private float m_accelerationSpeed;
    [SerializeField] private float m_deccelerationSpeed;
    [SerializeField] private float m_rotationSpeed;

    private Rigidbody m_rigidbody;
    private Vector2 m_movementInput;
    private Quaternion m_lookRotation;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if(m_movementInput != Vector2.zero)
            m_lookRotation = Quaternion.LookRotation(new Vector3(m_movementInput.x,0, m_movementInput.y), Vector3.up);
     
        transform.rotation = Quaternion.Slerp(transform.rotation, m_lookRotation, Time.deltaTime * m_rotationSpeed);
    }

    private void HandleMovement()
    {
        // Player Input
        Vector2 i = m_movementInput;
        // Current Veloctiy
        Vector3 v = m_rigidbody.velocity;

        // Check if there is input && (if input is more then check if velocity is more, else check if less)
        bool isAcceleratingX = i.x != 0 && (i.x > 0 ? v.x > 0 : v.x < 0);
        bool isAcceleratingZ = i.y != 0 && (i.y > 0 ? v.z > 0 : v.z < 0);

        float speedX = isAcceleratingX ? m_accelerationSpeed : m_deccelerationSpeed;
        float speedZ = isAcceleratingZ ? m_accelerationSpeed : m_deccelerationSpeed;



        // Input is already normalized by the input system
        Vector2 toVelocity = m_movementInput * m_maxMovementSpeed;
        Vector3 finalVelocity = m_rigidbody.velocity;

        // Add the acceleration
        finalVelocity.x = Mathf.MoveTowards(finalVelocity.x, toVelocity.x, speedX * Time.deltaTime);
        finalVelocity.z = Mathf.MoveTowards(finalVelocity.z, toVelocity.y, speedZ * Time.deltaTime);



        // Apply the velocitys
        m_rigidbody.velocity = finalVelocity;

#if UNITY_EDITOR
        Debug.DrawRay(transform.position, new Vector3(toVelocity.x, 0, toVelocity.y), Color.red);
        Debug.DrawRay(transform.position, m_rigidbody.velocity, Color.green);
#endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_maxMovementSpeed);
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        m_movementInput = context.ReadValue<Vector2>();
    }
}
