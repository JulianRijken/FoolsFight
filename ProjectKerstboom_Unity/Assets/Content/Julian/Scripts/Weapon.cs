using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{

    [Header("Weapon Style")]
    [SerializeField] private float m_bobScale;
    [SerializeField] private float m_bobSpeed;
    [SerializeField] private float m_bobOffset;
    [SerializeField] private float m_pickupSpeed;
    [SerializeField] private float m_toRotationSpeed;
    [SerializeField] private float m_timesRotationSpeedAfterDrop;
    private float m_rotateSpeed;
    private Transform m_weaponModel;


    [Header("Drop Movement")]
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_jumpHight;
    [SerializeField] private float m_minimalDistance;
    [SerializeField] private string m_spawnPointsParentTag;
    [SerializeField] private LayerMask m_blockLayer;
    [SerializeField] private AnimationCurve m_verticalCurve;
    private List<Vector3> m_points;
    private Transform m_spawnPointParent;


    [Header("Picking Up Weapon")]
    [SerializeField] private float m_pickupRange;
    [SerializeField] private LayerMask m_pickupLayer;
    private bool m_allowedToPickUp;


    public WeaponType m_WeaponType { get; private set; }


    private void Awake()
    {
        m_weaponModel = transform.GetChild(0);
        m_spawnPointParent = GameObject.FindGameObjectWithTag(m_spawnPointsParentTag).transform;

        m_allowedToPickUp = true;
    }

    private void Start()
    {
        m_points = new List<Vector3>();
        for (int i = 0; i < m_spawnPointParent.childCount; i++)
            m_points.Add(m_spawnPointParent.GetChild(i).position);

        transform.position = Vector3.zero;
    }

    private void Update()
    {
        if(m_allowedToPickUp)
        {
            HandleBobEffect();
            HandlePickingUpWeapon();
        }
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



    private void HandleBobEffect()
    {
        // Rotate the weapon
        m_rotateSpeed = Mathf.Lerp(m_rotateSpeed, m_toRotationSpeed, Time.deltaTime);
        m_weaponModel.Rotate(0, m_rotateSpeed * Time.deltaTime, 0);

        // Make the Weapon Bob up and down
        m_weaponModel.localPosition = new Vector3(0, (Mathf.Sin(Time.time * m_bobSpeed) * m_bobScale) + m_bobOffset, 0);
    }


    private void HandlePickingUpWeapon()
    {
        // Run a overlap sphere to see if any weapons are in the area
        Collider[] collisions = Physics.OverlapSphere(transform.position, m_pickupRange, m_pickupLayer);

        for (int i = 0; i < collisions.Length; i++)
        {
            PlayerController player = collisions[i].GetComponent<PlayerController>();


            // Check if the collider is an player
            if (player == null)
                continue;

            // Check if the player is the local one
            if (!player.photonView.IsMine)
                continue;

            // Check if the player is the local one
            if (!player.CanPickup)
                continue;

            PickupWeapon(player);

            // Exit out of the loop
            return;
        }
    }


    public void PickupWeapon(PlayerController player)
    {
        photonView.RPC("PickupWeaponRPC", RpcTarget.All, player.gameObject.GetPhotonView().ViewID);    
    }
    [PunRPC] private void PickupWeaponRPC(int targetView)
    {
        if (!m_allowedToPickUp)
            return;

        // Kill old movement
        m_weaponModel.DOKill();
        transform.DOKill();
        StopAllCoroutines();

        // Set the current weapon to the player
        PlayerController player = PhotonView.Find(targetView).GetComponent<PlayerController>();
        player.SetCurrentWeapon(this);

        // Set the parant of the weapon
        transform.SetParent(player.WeaponPivotPoint);

        // Move to hand
        m_weaponModel.DOLocalMove(Vector3.zero, m_pickupSpeed);
        m_weaponModel.DOLocalRotateQuaternion(Quaternion.identity, m_pickupSpeed);
        transform.DOLocalMove(Vector3.zero, m_pickupSpeed);
        transform.DOLocalRotateQuaternion(Quaternion.identity, m_pickupSpeed);

        // Make sure the weapon is not allowed to be picked up again
        m_allowedToPickUp = false;
    }


    public void DropWeapon()
    {
        photonView.RPC("DropWeaponRPC", RpcTarget.All, GetNextPoint());
    }
    [PunRPC] private void DropWeaponRPC(Vector3 nextPoint)
    {
        if (m_allowedToPickUp)
            return;

        // Kill old movement
        m_weaponModel.DOKill();
        transform.DOKill();
        StopAllCoroutines();

        // Reset the local weapon transform rotations
        transform.SetParent(null);
        m_weaponModel.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;

        // Set the rotation speed to extra so the hammer movement has more effect
        m_rotateSpeed = m_timesRotationSpeedAfterDrop * m_toRotationSpeed;

        // Start the corutine to move the hammer
        StartCoroutine(MoveToPoint(nextPoint));

        // Make sure the weapon is allowed to be picked up again
        m_allowedToPickUp = true;
    }


    public void ResetWeapon()
    {
        photonView.RPC("ResetWeaponRPC", RpcTarget.All);
    }
    [PunRPC] private void ResetWeaponRPC()
    {
        // Kill old movement
        m_weaponModel.DOKill();
        transform.DOKill();

        // Reset the local weapon transform rotations
        transform.SetParent(null);
        m_weaponModel.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;

        StopAllCoroutines();

        // Zero the position
        transform.position = Vector3.zero;

        // Reset the rotate speed
        m_rotateSpeed = m_toRotationSpeed;

        m_allowedToPickUp = true;

    }


    private Vector3 GetNextPoint()
    {
        // Get a list of all the next possible points
        List<Vector3> possiblePoints = new List<Vector3>();
        for (int i = 0; i < m_points.Count; i++)
        {
            float distance = Vector2.Distance(m_points[i], transform.position);
            if (distance > m_minimalDistance)
            {
                if (!Physics.Linecast(transform.position, m_points[i], m_blockLayer))
                {
                    possiblePoints.Add(m_points[i]);
                }
            }
        }

        // Get one of the possible points and check if there is any
        Vector3 toPoint = Vector3.zero;

        if (possiblePoints.Count > 0)
            toPoint = possiblePoints[Random.Range(0, possiblePoints.Count)];
        else
            Debug.LogError("No Points Found");

        return toPoint;
    }


    private IEnumerator MoveToPoint(Vector3 toPoint)
    {
        Vector3 fromPoint = transform.position;
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime * m_moveSpeed;

            Vector3 niewPosition = Vector3.Lerp(fromPoint, toPoint, time);
            niewPosition.y = (m_verticalCurve.Evaluate(time) * m_jumpHight) + toPoint.y;

            transform.position = niewPosition;

            yield return new WaitForEndOfFrame();
        }

        transform.position = toPoint;
    }





}

public enum WeaponType
{
    Hammer
}
