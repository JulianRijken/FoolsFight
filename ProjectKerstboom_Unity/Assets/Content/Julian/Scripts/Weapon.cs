using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Weapon : MonoBehaviour
{

    [SerializeField] private float m_bobScale;
    [SerializeField] private float m_bobSpeed;
    [SerializeField] private float m_bobOffset;
    [SerializeField] private float m_toRotationSpeed;
    [SerializeField] private float m_timesRotationSpeedAfterDrop;
    [SerializeField] private float m_pickupDelay;

    public bool m_allowedToPickUp = true;

    private Transform m_weaponModel;
    private Rigidbody m_rigidbody;
    private float m_rotateSpeed;


    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_weaponModel = transform.GetChild(0);
    }

    private void Update()
    {
        if (m_allowedToPickUp)
        {
            m_rotateSpeed = Mathf.Lerp(m_rotateSpeed, m_toRotationSpeed, Time.deltaTime);
            m_weaponModel.Rotate(0, m_rotateSpeed * Time.deltaTime, 0);
            m_weaponModel.localPosition = new Vector3(0, (Mathf.Sin(Time.time * m_bobSpeed) * m_bobScale) + m_bobOffset, 0);
        }
    }

    public void PickupWeapon(PlayerController owner, Transform parent, float pickupTime)
    {

        // Set the parant of the weapon
        transform.SetParent(parent);
        m_rigidbody.isKinematic = true;

        m_weaponModel.DOLocalMove(Vector3.zero, pickupTime);
        m_weaponModel.DOLocalRotateQuaternion(Quaternion.identity, pickupTime);
        transform.DOLocalMove(Vector3.zero, pickupTime);
        transform.DOLocalRotateQuaternion(Quaternion.identity, pickupTime);

        m_allowedToPickUp = false;
    }

    public void DropWeapon()
    {
        float sidewaysForce = 4f;
        float upwardsForce = 7f;

        m_weaponModel.DOKill();
        transform.DOKill();

        transform.SetParent(null);
        m_rigidbody.isKinematic = false;

        m_rigidbody.velocity = new Vector3(Random.Range(-1f, 1f) * sidewaysForce, upwardsForce, Random.Range(-1f, 1f) * sidewaysForce);
        m_rotateSpeed = m_timesRotationSpeedAfterDrop * m_toRotationSpeed;

        m_allowedToPickUp = true;
    }




}
