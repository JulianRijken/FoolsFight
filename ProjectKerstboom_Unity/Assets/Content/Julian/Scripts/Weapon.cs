using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{

    [Header("Ground Settings")]
    [SerializeField] private float m_bobScale;
    [SerializeField] private float m_bobSpeed;
    [SerializeField] private float m_bobOffset;
    [SerializeField] private float m_pickupSpeed;
    [SerializeField] private float m_toRotationSpeed;
    [SerializeField] private float m_timesRotationSpeedAfterDrop;

    [Header("Jump Settings")]
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_jumpHight;
    [SerializeField] private float m_minimalDistance;
    [SerializeField] private string m_spawnPointsParentTag;
    [SerializeField] private LayerMask m_blockLayer;
    [SerializeField] private AnimationCurve m_verticalCurve;

    private List<Vector3> m_points;
    private Transform m_spawnPointParent;

    [Header("Weapon Pickup")]
    private Transform m_weaponModel;
    private SphereCollider m_collider;
    private float m_rotateSpeed;

    public WeaponType m_WeaponType { get; private set; }
    public bool m_allowedToPickUp { get; private set; }


    private void Awake()
    {
        m_collider = GetComponent<SphereCollider>();
        m_weaponModel = transform.GetChild(0);
        m_spawnPointParent = GameObject.FindGameObjectWithTag(m_spawnPointsParentTag).transform;

        m_allowedToPickUp = true;
    }

    private void Start()
    {
        m_points = new List<Vector3>();
        for (int i = 0; i < m_spawnPointParent.childCount; i++)
            m_points.Add(m_spawnPointParent.GetChild(i).position);

        transform.position = m_points[Random.Range(0, m_points.Count)];
    }

    private void Update()
    {
        HandleBobEffect();
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
        if (!m_allowedToPickUp)
            return;

        // Rotate the weapon
        m_rotateSpeed = Mathf.Lerp(m_rotateSpeed, m_toRotationSpeed, Time.deltaTime);
        m_weaponModel.Rotate(0, m_rotateSpeed * Time.deltaTime, 0);

        // Make the Weapon Bob up and down
        m_weaponModel.localPosition = new Vector3(0, (Mathf.Sin(Time.time * m_bobSpeed) * m_bobScale) + m_bobOffset, 0);
    }



    public void PickupWeapon(Transform parent)
    {
        if (m_allowedToPickUp) 
            photonView.RPC("PickupWeaponRPC", RpcTarget.All, parent.gameObject.GetPhotonView().ViewID);
    }
    [PunRPC]
    private void PickupWeaponRPC(int targetView)
    {
        Transform parent = PhotonView.Find(targetView).transform;

        Debug.Log(parent.name);

        // Set the parant of the weapon
        transform.SetParent(parent);
        m_collider.enabled = false;

        m_weaponModel.DOLocalMove(Vector3.zero, m_pickupSpeed);
        m_weaponModel.DOLocalRotateQuaternion(Quaternion.identity, m_pickupSpeed);
        transform.DOLocalMove(Vector3.zero, m_pickupSpeed);
        transform.DOLocalRotateQuaternion(Quaternion.identity, m_pickupSpeed);

        m_allowedToPickUp = false;
    }



    public void DropWeapon()
    {
        if (!m_allowedToPickUp)
            photonView.RPC("DropWeaponRPC", RpcTarget.All, GetNextPoint());
    }
    [PunRPC]
    private void DropWeaponRPC(Vector3 nextPoint)
    {
        m_weaponModel.DOKill();
        transform.DOKill();

        // Reset Weapon to fit the ground state
        transform.SetParent(null);
        m_collider.enabled = true;

        m_rotateSpeed = m_timesRotationSpeedAfterDrop * m_toRotationSpeed;

        m_allowedToPickUp = true;


        m_weaponModel.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;
        // =================================

        // Start the corutine to move the hammer
        StartCoroutine(MoveToPoint(nextPoint));
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
