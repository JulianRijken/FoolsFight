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


    public WeaponType m_WeaponType { get; private set; }
    public bool m_allowedToPickUp { get; private set; }

    private Transform m_weaponModel;
    private Rigidbody m_rigidbody;
    private PhotonRigidbodyView m_rigidbodyView;
    private SphereCollider m_collider;
    private float m_rotateSpeed;


    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<SphereCollider>();
        m_rigidbodyView = GetComponent<PhotonRigidbodyView>();
        m_weaponModel = transform.GetChild(0);

        m_allowedToPickUp = true;
    }

    private void Update()
    {
        if (m_allowedToPickUp)
        {
            // Rotate the weapon
            m_rotateSpeed = Mathf.Lerp(m_rotateSpeed, m_toRotationSpeed, Time.deltaTime);
            m_weaponModel.Rotate(0, m_rotateSpeed * Time.deltaTime, 0);

            // Make the Weapon Bob up and down
            m_weaponModel.localPosition = new Vector3(0, (Mathf.Sin(Time.time * m_bobSpeed) * m_bobScale) + m_bobOffset, 0);

            if (transform.position.y < 5)
                transform.position = Vector3.zero + Vector3.up * 5f;
        }
    }


    public void PickupWeapon(Transform parent)
    {
        photonView.RPC("PickupWeaponRPC", RpcTarget.All, parent.gameObject.GetPhotonView().ViewID);
    }

    [PunRPC]
    private void PickupWeaponRPC(int targetView)
    {
        Transform parent = PhotonView.Find(targetView).transform;

        Debug.Log(parent.name);

        // Set the parant of the weapon
        transform.SetParent(parent);
        m_rigidbody.isKinematic = true;
        m_rigidbodyView.enabled = false;
        m_collider.enabled = false;

        m_weaponModel.DOLocalMove(Vector3.zero, m_pickupSpeed);
        m_weaponModel.DOLocalRotateQuaternion(Quaternion.identity, m_pickupSpeed);
        transform.DOLocalMove(Vector3.zero, m_pickupSpeed);
        transform.DOLocalRotateQuaternion(Quaternion.identity, m_pickupSpeed);

        m_allowedToPickUp = false;
    }

    public void DropWeapon()
    {
        photonView.RPC("DropWeaponRPC", RpcTarget.All);
    }

    [PunRPC]
    private void DropWeaponRPC()
    {
        float sidewaysForce = 6f;
        float upwardsForce = 11f;

        m_weaponModel.DOKill();
        transform.DOKill();

        transform.SetParent(null);
        m_rigidbody.isKinematic = false;
        m_rigidbodyView.enabled = true;
        m_collider.enabled = true;

        m_rigidbody.velocity = new Vector3(Random.Range(-1f, 1f) * sidewaysForce, upwardsForce, Random.Range(-1f, 1f) * sidewaysForce);
        m_rotateSpeed = m_timesRotationSpeedAfterDrop * m_toRotationSpeed;

        m_allowedToPickUp = true;


        m_weaponModel.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;
    }
}

public enum WeaponType
{
    Hammer
}
