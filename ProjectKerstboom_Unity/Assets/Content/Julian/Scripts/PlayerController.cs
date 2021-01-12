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
    [SerializeField] private float m_dashDistance;
    [SerializeField] private float m_dashSpeed;
    [SerializeField] private float m_dashCooldown;
    [SerializeField] private LayerMask m_levelBlocklayer;
    private Vector2 m_movementInput;
    private Rigidbody m_rigidbody;
    private Quaternion m_lookRotation;
    private PlayerInput m_playerInput;
    private bool m_canDash = true;

    [Header("Ground Check")]
    [SerializeField] private float m_maxGroundCheckDistance;
    [SerializeField] private float m_radius;
    [SerializeField] private Vector3 m_groundCastOffset;
    [SerializeField] private LayerMask m_groundLayer;


    [Header("Weapons")]
    [SerializeField] private float m_pickupCooldown;
    [SerializeField] private Transform m_weaponPivotPoint;

    [SerializeField] private LayerMask m_damageLayer;
    [SerializeField] private LayerMask m_weaponBlockLayer;

    [SerializeField] private float m_hammerHitSize;
    [SerializeField] private float m_hammerDistanceOffset;

    private Weapon m_currentWeapon = null;
    private bool m_canPickup = true;


    [Header("Animator")]
    [SerializeField] private Animator m_animator;


    [Header("Multiplayer")]
    private bool m_isMine = true;

    [Header("Model")]
    [SerializeField] private Mesh[] m_possibleCharacterMeshes;
    [SerializeField] private Material[] m_possibleCharacterMaterials;
    private SkinnedMeshRenderer m_playerSkinnedMeshRender;


    [Header("General")]
    private bool m_isAlive = true;
    private PlayerState m_playerState = PlayerState.InActive;
    private CapsuleCollider m_collider;


    // Action for other scripts to use to check when new players spawn
    public static System.Action<PlayerController> m_onPlayerStarted;
    // Action for other scripts to check if the player died
    public static System.Action<PlayerController> m_onPlayerDeath;
    // Action for other scripts to check if the player got Destroyed
    public static System.Action<PlayerController> m_onPlayerDestroyed;


    private void Awake()
    {
        // Getting All componenets
        m_rigidbody = GetComponent<Rigidbody>();
        m_playerInput = GetComponent<PlayerInput>();
        m_collider = GetComponent<CapsuleCollider>();
        m_playerSkinnedMeshRender = GetComponent<SkinnedMeshRenderer>();

        // Give the player a random skin
        m_playerSkinnedMeshRender.material = m_possibleCharacterMaterials[Random.Range(0, m_possibleCharacterMaterials.Length)];
        m_playerSkinnedMeshRender.sharedMesh = m_possibleCharacterMeshes[Random.Range(0, m_possibleCharacterMeshes.Length)];


        SetPlayerState(PlayerState.InActive);

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

    private void OnDestroy()
    {
        if (m_currentWeapon != null)
        {
            m_onPlayerDestroyed?.Invoke(this);
            m_currentWeapon.DropWeapon();
        }
    }

    private void Update()
    {
        if (!m_isMine)
            return;

        HandleMovement();
        HandleRotation();
        HandleGroundCheck();
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
        // Dont move the player if he is dashing
        if (m_playerState == PlayerState.Dashing)
            return;

        if (m_movementInput != Vector2.zero)
            m_lookRotation = Quaternion.LookRotation(new Vector3(m_movementInput.x, 0, m_movementInput.y), Vector3.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, m_lookRotation, Time.deltaTime * m_rotationSpeed);
    }

    private void HandleMovement()
    {
        // Dont move the player if he is dashing
        if (m_playerState == PlayerState.Dashing)
            return;

        // Cancle teh movment if the player isent active
        if (m_playerState == PlayerState.InActive || m_playerState == PlayerState.Dead)
        {
            // Apply the velocitys
            m_rigidbody.velocity = Vector3.zero;
            return;
        }

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
        // Only ground check when the player is active
        if (m_playerState == PlayerState.InActive)
            return;

        Ray ray = new Ray(transform.position + m_groundCastOffset, Vector3.down);

#if UNITY_EDITOR
        // Check the ray
        Debug.DrawLine(ray.origin, ray.direction * m_maxGroundCheckDistance, Color.green);
#endif

        if (!Physics.SphereCast(ray, m_radius,m_maxGroundCheckDistance,m_groundLayer))
        {
            OnDeath("The ground");
        }
       
    }


    #endregion


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
    [PunRPC]
    private void FireWeaponRPC()
    {
        m_animator.SetTrigger("Fire");
    }

    private void Dash()
    {

        // Only dash if the player is waking
        if (m_playerState != PlayerState.Walking)
            return;

        // make sure the player can dash
        if (!m_canDash)
            return;

        SetPlayerState(PlayerState.Dashing);

        // Create a variable for the final destination 
        Vector3 destination;
        Vector3 origin = m_collider.center + transform.position;
        Vector3 direction = transform.forward;

        // Check with a raycast if there is a wall in the way of the destination
        RaycastHit raycastHit;
        if (Physics.Raycast(origin, direction, out raycastHit, m_dashDistance, m_levelBlocklayer))
        {
            // If the raycast hit a wall then move the player untill the wall
            destination = raycastHit.point - (direction * m_collider.radius);
        }
        else
        {
            // if the raycast hit nothing move the max distance
            destination = origin + (direction * m_dashDistance);
        }

        // Debug the dash ray
        Debug.DrawLine(origin, destination, Color.cyan, 10f);

        StartCoroutine(DashEnumerator(destination));
    }

    private IEnumerator DashEnumerator(Vector3 destination)
    {
        // Set the lerp positon to the local start position
        Vector3 lerpPosition = transform.position;

        while (true)
        {
            // move the lerp position up
            lerpPosition = Vector3.MoveTowards(lerpPosition, destination, Time.deltaTime * m_dashSpeed);

            // Get a final position and flatten the y
            Vector3 movePosition = lerpPosition;
            movePosition.y = 0;

            // Move the actual player rigidbody 
            m_rigidbody.MovePosition(movePosition);

            // Breake out of the loop as soon as the position is reached 
            if (lerpPosition == destination)
            {
                break;
            }


            // wait for the eind of the frame to create a frame loop
            yield return new WaitForEndOfFrame();
        }

        // End the dash
        SetPlayerState(PlayerState.Walking);
        AddDashDelay();
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

    private void AddDashDelay()
    {
        StartCoroutine(DashDelay());
    }
    private IEnumerator DashDelay()
    {
        m_canDash = false;
        yield return new WaitForSeconds(m_dashCooldown);
        m_canDash = true;
    }


    private void OnWeaponUsed()
    {

        Collider[] hits;
        Vector3 checkPosition = transform.position + (transform.forward * m_hammerDistanceOffset);
        hits = Physics.OverlapSphere(checkPosition, m_hammerHitSize);

        if (Physics.Linecast(transform.position, checkPosition, m_weaponBlockLayer))
            return;

        for (int i = 0; i < hits.Length; i++)
        {
            // Check if it's not this player
            if (hits[i].transform == transform)
                continue;

            // Check if what is hit is a damageble object
            IDamageable damageable = hits[i].GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            // then hit the object
            damageable.OnDeath($"{photonView.Owner.NickName} {gameObject.name} Hammer");
        }

        DropCurrentWeapon();
        AddPickupDelay();
    }

    public void OnDeath(string damagedBy)
    {
        photonView.RPC("OnDeathRPC", RpcTarget.All);
    }
    [PunRPC]
    private void OnDeathRPC()
    {
        SetPlayerState(PlayerState.Dead);

        // Call the player death action last
        m_onPlayerDeath?.Invoke(this);
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

    public SkinnedMeshRenderer PlayerMeshRenderer
    {
        get
        {
            return m_playerSkinnedMeshRender;
        }
    }

    public void SetPlayerState(PlayerState newState)
    {
        m_playerState = newState;

        switch (newState)
        {
            case PlayerState.Walking:

                m_canPickup = true;
                m_isAlive = true; 
                gameObject.SetActive(true);

                break;
            case PlayerState.Dashing:

                m_canPickup = true;
                m_isAlive = true;
                gameObject.SetActive(true);

                break;
            case PlayerState.InActive:

                m_movementInput = Vector3.zero;
                m_canPickup = false;
                m_isAlive = true;
                m_canDash = true;
                gameObject.SetActive(true);

                break;
            case PlayerState.Dead:

                StopAllCoroutines();
                DropCurrentWeapon();

                m_canPickup = false;
                m_isAlive = false;
                gameObject.SetActive(false);

                break;
            case PlayerState.Disabled:

                StopAllCoroutines();
                gameObject.SetActive(false);
                photonView.Synchronization = ViewSynchronization.Off;

                break;
            default:
                break;
        }

    }



    #region Input

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        m_movementInput = context.ReadValue<Vector2>();
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (m_playerState == PlayerState.InActive)
            return;

        FireWeapon();
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (m_playerState == PlayerState.InActive)
            return;

        Dash();
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

    public enum PlayerState
    {
        Walking,
        InActive,
        Dead,
        Dashing,
        Disabled
    }


}
