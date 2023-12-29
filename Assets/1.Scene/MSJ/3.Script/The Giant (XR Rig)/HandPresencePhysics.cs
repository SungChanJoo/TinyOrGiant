using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum HandPresenceType
{
    LeftHand,
    RightHand,
}

public class HandPresencePhysics : NetworkBehaviour
{
    public HandPresenceType handType;

    [Header("Non-Physical Hand")]
    public bool showNonPhysicalHand = true;
    public float distanceThreshold = 3f;
    Transform target;
    Renderer nonPhysicalHandRenderer;

    Rigidbody rb;

    private void Awake()
    {
        if (!isLocalPlayer) return;

        switch (handType)
        {
            case HandPresenceType.LeftHand:
                target = GameObject.FindGameObjectWithTag("NonPhysicsLeftHandPresence").transform;
                nonPhysicalHandRenderer = GameObject.FindGameObjectWithTag("NonPhysicsLeftHandRenderer").GetComponent<SkinnedMeshRenderer>();
                break;
            case HandPresenceType.RightHand:
                target = GameObject.FindGameObjectWithTag("NonPhysicsRightHandPresence").transform;
                nonPhysicalHandRenderer = GameObject.FindGameObjectWithTag("NonPhysicsRightHandRenderer").GetComponent<SkinnedMeshRenderer>();
                break;
        }

        TryGetComponent(out rb);
    }

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        TryMoveHand();
        TryRotateHand();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            showNonPhysicalHand = false;
            return;
        }

        ShowNonPhysicalHand();
    }

    private void TryMoveHand()
    {
        // Try move to target position
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;
    }

    private void TryRotateHand()
    {
        // Try rotate to target rotation
        Quaternion rotationDiff = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDiff.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotationDiffInDegree = angleInDegree * rotationAxis;

        rb.angularVelocity = (rotationDiffInDegree * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void ShowNonPhysicalHand()
    {
        if (showNonPhysicalHand)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            nonPhysicalHandRenderer.enabled = distance < distanceThreshold ? false : true;
        }
    }
    
}
