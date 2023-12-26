using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandPresenceType
{
    LeftHand,
    RightHand,
}

public class HandPresencePhysics : MonoBehaviour
{
    public HandPresenceType handType;

    [Header("Non-Physical Hand")]
    public bool showNonPhysicalHand = true;
    public float distanceThreshold = .3f;
    Transform target;
    Renderer nonPhysicalHandRenderer;

    [Header("Physical Hand")]
    public float colliderEnableDelay = .5f;
    Collider[] handColliders;
    Rigidbody rb;

    private void Awake()
    {
        switch (handType)
        {
            case HandPresenceType.LeftHand:
                target = GameObject.FindGameObjectWithTag("NonPhysicsLeftHandPresence").transform;
                nonPhysicalHandRenderer = GameObject.FindGameObjectWithTag("NonPhysicsLeftHandRenderer").GetComponent<MeshRenderer>();
                break;
            case HandPresenceType.RightHand:
                target = GameObject.FindGameObjectWithTag("NonPhysicsRightHandPresence").transform;
                nonPhysicalHandRenderer = GameObject.FindGameObjectWithTag("NonPhysicsRightHandRenderer").GetComponent<MeshRenderer>();
                break;
        }

        TryGetComponent(out rb);
        handColliders = GetComponentsInChildren<Collider>();
    }

    private void FixedUpdate()
    {
        // Try move to target position
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;

        // Try rotate to target rotation
        Quaternion rotationDiff = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDiff.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotationDiffInDegree = angleInDegree * rotationAxis;

        rb.angularVelocity = (rotationDiffInDegree * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (showNonPhysicalHand)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            nonPhysicalHandRenderer.enabled = distance < distanceThreshold ? false : true;
        }
    }

    // XR Direct Interactor Event
    public void ToggleCollider(bool isEnable)
    {
        if (isEnable)
            Invoke(nameof(EnableHandCollider), colliderEnableDelay);
        else
            DisableHandCollider();
    }

    public void EnableHandCollider()
    {
        foreach (var handCollider in handColliders)
        {
            handCollider.enabled = true;
        }
    }

    public void DisableHandCollider()
    {
        foreach (var handCollider in handColliders)
        {
            handCollider.enabled = false;
        }
    }
}
