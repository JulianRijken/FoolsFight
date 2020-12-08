using Photon.Pun;
using Photon.Realtime;
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


    [Header("Ground Check")]
    [SerializeField] private float m_maxGroundCheckDistance;
    [SerializeField] private float m_radius;
    [SerializeField] private Vector3 m_castOffset;
    [SerializeField] private LayerMask m_groundLayer;


    [Header("Weapons")]
    [SerializeField] private float m_pickupCooldown;
    [SerializeField] private LayerMask m_damageLayer;
    [SerializeField] private Transform m_weaponPivotPoint;
    private Weapon m_currentWeapon = null;
    private bool m_canPickup = true;


    [Header("Animator")]
    [SerializeField] private Animator m_animator;


    [Header("Multiplayer")]
    private bool m_isMine = true;


    [Header("General")]
    private bool m_isAlive = true;


    // Action for other scripts to use to check when new players spawn
    public static System.Action<PlayerController> m_onPlayerStarted;
    // Action for other scripts to check if the player died
    public static System.Action m_onPlayerDeath;


    private void Awake()
    {
        // Getting All componenets
        m_rigidbody = GetComponent<Rigidbody>();
        m_playerInput = GetComponent<PlayerInput>();

        m_canPickup = true;

        // Checking if this is mine
        if (photonView != null)
        {
            m_isMine = photonView.IsMine;
        }

        if(!m_isMine)
        {
            Destroy(m_playerInput);
            Destroy(m_rigidbody);
        }
    }

    private void Start()
    {
        // Always call player started even if not mine
        m_onPlayerStarted?.Invoke(this);
    }

    private void Update()
    {
        // If this is my character move it
        if (m_isMine)
        {
            HandleMovement();
            HandleRotation();
            HandleGroundCheck();
        }
    }


    public override void OnEnable()
    {
        base.OnEnable();

        // Only subscribe events if this one is mine
        if(m_isMine)
        {
            PlayerAnimatorPass.m_onWeaponUsed += OnWeaponUsed;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Always unsubscribe all events
        PlayerAnimatorPass.m_onWeaponUsed -= OnWeaponUsed;
    }


    #region Movement

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

    private void HandleGroundCheck()
    {
        Ray ray = new Ray(transform.position + m_castOffset, Vector3.down);

#if UNITY_EDITOR
        // Check the ray
        Debug.DrawLine(ray.origin, ray.direction * m_maxGroundCheckDistance, Color.green);
#endif

        if (!Physics.SphereCast(ray, m_radius,m_maxGroundCheckDistance,m_groundLayer))
        {
            OnHit("The ground");
        }
       
    }



    public void ReSpawn()
    {
        photonView.RPC("ReSpawnRPC", RpcTarget.All);
    }
    [PunRPC]
    private void ReSpawnRPC()
    {
        Debug.Log("Player Reset");

        transform.position = Vector3.zero;
        m_isAlive = true;
        gameObject.SetActive(true);
    }

    #endregion


    #region Weapon Handeling

    private void DropCurrentWeapon()
    {
        // Check if the player has a weapon
        if (m_currentWeapon == null)
            return;

        if (m_isMine)
            m_currentWeapon.DropWeapon();

        m_currentWeapon = null;
    }

    private void FireWeapon()
    {
        if (m_currentWeapon == null)
            return;

        m_animator.SetTrigger("Fire");

        photonView.RPC("FireWeaponRPC", RpcTarget.Others);
    }

    private void AddPickupDelay()
    {
        if(gameObject.activeSelf)
            StartCoroutine(PickupDelay());
    }

    private IEnumerator PickupDelay()
    {
        m_canPickup = false;
        yield return new WaitForSeconds(m_pickupCooldown);
        m_canPickup = true;
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
            damageable.OnHit($"{photonView.Owner.NickName} {gameObject.name} Hammer");
        }

        DropCurrentWeapon();
        AddPickupDelay();
    }
    [PunRPC]
    private void FireWeaponRPC()
    {
        m_animator.SetTrigger("Fire");
    }

    public void OnHit(string damagedBy)
    {
        photonView.RPC("OnHitRPC", RpcTarget.All, photonView.ViewID, damagedBy);
    }
    [PunRPC]
    private void OnHitRPC(int viewID,string damagedBy)
    {    
        DropCurrentWeapon();

        m_isAlive = false;
        gameObject.SetActive(false);
        m_onPlayerDeath?.Invoke();
    }


    public void SetCurrentWeapon(Weapon newWeapon)
    {
        m_currentWeapon = newWeapon;
    }

    public Transform WeaponPivotPoint
    {
        get
        {
            return m_weaponPivotPoint;
        }
    }

    public bool CanPickup
    {
        get
        {
            return m_canPickup;
        }
    }

    public bool IsAlive 
    { 
        get 
        {
            return m_isAlive; 
        } 
    }


    #endregion


    #region Input

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

    #endregion


#if UNITY_EDITOR
    // Checking Movement
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_maxMovementSpeed);
    }
#endif

}
