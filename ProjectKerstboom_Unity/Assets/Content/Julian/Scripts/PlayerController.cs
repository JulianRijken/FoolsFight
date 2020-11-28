using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{

    [Header("Movement")]
    [SerializeField] private float m_maxMovementSpeed;
    [SerializeField] private float m_accelerationSpeed;
    [SerializeField] private float m_deccelerationSpeed;
    [SerializeField] private float m_rotationSpeed;
    private Vector2 m_movementInput;
    private Rigidbody m_rigidbody;
    private Quaternion m_lookRotation;
    private PlayerInput m_playerInput;


    [Header("Weapons")]
    [SerializeField] private float m_pickupCooldown;
    [SerializeField] private LayerMask m_damageLayer;
    [SerializeField] private Transform m_weaponPivotPoint;
    private Weapon m_currentWeapon = null;
    private bool m_canPickup = true;


    [Header("Animator")]
    [SerializeField] private Animator m_animator;


    [Header("Multiplayer")]
    [SerializeField] private bool isMine = true;


    // Action for other scripts to use to check when new players spawn
    public static System.Action<Transform> m_onPlayerStarted;


    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_playerInput = GetComponent<PlayerInput>();

        if (photonView != null)
            isMine = photonView.IsMine;

        if (isMine)
        {
            PlayerAnimatorPass.m_onWeaponUsed += OnWeaponUsed;
        }
        else
        {
            m_playerInput.enabled = false;
            Destroy(m_rigidbody);
        }
    }

    private void Start()
    {
        m_onPlayerStarted?.Invoke(transform);
    }

    private void OnDestroy()
    {
        if (!isMine)
            return;

        PlayerAnimatorPass.m_onWeaponUsed -= OnWeaponUsed;
    }

    private void Update()
    {
        if (!isMine)
            return;

        HandleMovement();
        HandleRotation();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_maxMovementSpeed);
    }
#endif


    private void HandleRotation()
    {
        if (m_movementInput != Vector2.zero)
            m_lookRotation = Quaternion.LookRotation(new Vector3(m_movementInput.x, 0, m_movementInput.y), Vector3.up);

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


    private void DropCurrentWeapon()
    {
        // Check if the player has a weapon
        if (m_currentWeapon == null)
            return;

        m_currentWeapon.DropWeapon();
        m_currentWeapon = null;
    }

    private void FireWeapon()
    {
        if (m_currentWeapon == null)
            return;

        m_animator.SetTrigger("Fire");
    }

    private void OnWeaponUsed()
    {
        Vector3 orgin = transform.position;
        Vector3 halfExtends = Vector3.one;
        Vector3 direction = transform.forward;
        Quaternion rotation = Quaternion.identity;
        float distance = 2;

        RaycastHit[] hits = Physics.BoxCastAll(orgin, halfExtends, direction, rotation, distance, m_damageLayer);


        for (int i = 0; i < hits.Length; i++)
        {
            // Check if it's not this player
            if (hits[i].transform == transform)
                continue;

            // Check if what is hit is a damageble object
            IDamageable damageable = hits[i].collider.GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            // then hit the object
            damageable.OnHit(gameObject.name);
        }

        DropCurrentWeapon();
        AddPickupDelay();
    }


    private void AddPickupDelay()
    {
        StartCoroutine(PickupDelay());
    }

    private IEnumerator PickupDelay()
    {
        m_canPickup = false;
        yield return new WaitForSeconds(m_pickupCooldown);
        m_canPickup = true;
    }


    public void OnHit(string damagedBy)
    {
        photonView.RPC("OnHitRPC", RpcTarget.All, photonView.ViewID);
    }
    [PunRPC]
    private void OnHitRPC(int viewID)
    {
        Transform diedPlayer = PhotonView.Find(viewID).transform;
        diedPlayer.position = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
    }

    public void SetCurrentWeapon(Weapon newWeapon)
    {
        m_currentWeapon = newWeapon;
    }

    public bool CanPickupWeapon()
    {
        return m_canPickup;
    }

    public Transform GetWeaponPivotPoint()
    {
        return m_weaponPivotPoint;
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        m_movementInput = context.ReadValue<Vector2>();
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        FireWeapon();
    }
}
