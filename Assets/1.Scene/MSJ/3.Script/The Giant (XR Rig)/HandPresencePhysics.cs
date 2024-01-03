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
    private HandPresence handPresence;

    [Header("Magical Punch")]
    public GameObject magicalPunchProjectile;
    public Transform rayInteractor;
    [Range(1f, 90f)] public float punchForwardAngleThreshold = 80f;
    [Range(1f, 5f)] public float punchVelocityThreshold = 2f;
    [Range(.1f, 10f)] public float punchCoolDown = .5f;
    public float curPunchCoolDown = 0f;

    [Header("Gesture Detection")]
    public bool isClaw = false;
    public float clawThreshold = .7f;

    public bool isFist = false;
    public float fistThreshold = .7f;

    [Header("Non-Physical Hand")]
    public bool showNonPhysicalHand = true;
    public float distanceThreshold = 3f;
    Transform target;
    Renderer nonPhysicalHandRenderer;

    Rigidbody rb;

    private void Awake()
    {
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
        handPresence = GetComponent<HandPresence>();
    }

    private IEnumerator Start()
    {
        while (isLocalPlayer)
        {
            curPunchCoolDown = Mathf.Max(0, curPunchCoolDown - Time.deltaTime);
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        TryMoveHand();
        TryRotateHand();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        CheckHandGesture();
        CheckMagicPunch();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;

        ShowNonPhysicalHand();
    }

    private void CheckHandGesture()
    {
        var clawValue = handPresence.handAnimator.GetFloat("Trigger");
        var fistValue = handPresence.handAnimator.GetFloat("Grip");

        isClaw = clawValue >= clawThreshold;
        isFist = fistValue >= fistThreshold;
    }

    Vector3 lastTargetPos = Vector3.zero;
    private void CheckMagicPunch()
    {
        Vector3 currentTargetPosition = target.position;

        if (!isFist)
        {
            lastTargetPos = currentTargetPosition;
            return;
        }

        Vector3 desiredMove = currentTargetPosition - lastTargetPos;
        Vector3 desiredDirection = desiredMove.normalized;

        if (desiredMove.magnitude >= punchVelocityThreshold                                         // Check desired move vector
            && curPunchCoolDown == 0                                                                // Check cool down
            && Vector3.Angle(transform.forward, desiredDirection) <= punchForwardAngleThreshold)    // Check desired direction

        {
            var projectileObj = Instantiate(magicalPunchProjectile, rayInteractor.position, rayInteractor.rotation);
            NetworkServer.Spawn(projectileObj);
            curPunchCoolDown = punchCoolDown;
        }

        lastTargetPos = currentTargetPosition;
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
