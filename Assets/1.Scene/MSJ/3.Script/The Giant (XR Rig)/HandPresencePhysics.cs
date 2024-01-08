using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.XR.Interaction.Toolkit;

public enum HandPresenceType
{
    LeftHand,
    RightHand,
}

public class HandPresencePhysics : NetworkBehaviour
{
    public ActionBasedController controller;
    public HandPresenceType handType;
    private HandPresence handPresence;

    [Header("Pose Detection")]
    public bool isClawPose = false;
    public float clawThreshold = .7f;

    public bool isFistPose = false;
    public float fistThreshold = .7f;

    public bool isMagicPunchPose = false;
    [Range(30f, 90f)] public float punchAngleThreshold = 60f;

    public bool isEarthquakePose = false;
    [Range(30f, 90f)] public float earthquakeAngleThreshold = 60f;

    [Header("Magical Punch")]
    [Range(0f, 1f)] public float magicalPunch_hapticIntensity = 0f;
    [Range(0f, 5f)] public float magicalPunch_hapticDuration = 0f;

    public Transform rayInteractor; // 사용하지 않도록 변경 필요
    public GameObject magicalPunchProjectile;

    [Range(30f, 90f)] public float punchForwardAngleThreshold = 80f;
    [Range(1f, 5f)] public float punchVelocityThreshold = 2f;
    [Range(.1f, 10f)] public float punchCoolDown = .5f;
    public float curPunchCoolDown = 0f;

    [Header("Earth Quake")]
    [Range(0f, 1f)] public float earthquake_hapticIntensity = 0f;
    [Range(0f, 5f)] public float earthquake_hapticDuration = 0f;

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

        CheckHandPose();
        CheckMagicalPunch();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;

        ShowNonPhysicalHand();
    }

    private void CheckHandPose()
    {
        var clawValue = handPresence.handAnimator.GetFloat("Trigger");
        var fistValue = handPresence.handAnimator.GetFloat("Grip");
        isClawPose = clawValue >= clawThreshold;
        isFistPose = fistValue >= fistThreshold;

        Vector3 handHorizontalDirection = handType == HandPresenceType.RightHand ? transform.right : -transform.right;
        isMagicPunchPose = isFistPose && Vector3.Angle(handHorizontalDirection, Vector3.up) <= punchAngleThreshold;
        isEarthquakePose = isFistPose && Vector3.Angle(transform.up, Vector3.up) <= earthquakeAngleThreshold;
    }

    Vector3 lastTargetPos = Vector3.zero;
    private void CheckMagicalPunch()
    {
        Vector3 currentTargetPosition = target.position;
        if (!isFistPose)
        {
            lastTargetPos = currentTargetPosition;
            return;
        }

        Vector3 desiredMove = target.position - lastTargetPos;
        if (IsValidPunch(desiredMove))
        {
            var projectileObj = Instantiate(magicalPunchProjectile, rayInteractor.position, rayInteractor.rotation);
            NetworkServer.Spawn(projectileObj);
            curPunchCoolDown = punchCoolDown;

            controller.SendHapticImpulse(magicalPunch_hapticIntensity, magicalPunch_hapticDuration);
        }

        lastTargetPos = currentTargetPosition;
    }

    private bool IsValidPunch(Vector3 desiredMove)
    {
        var desiredDirection = desiredMove.normalized;

        if (!isMagicPunchPose
            || isEarthquakePose
            || curPunchCoolDown > 0
            || desiredMove.magnitude < punchVelocityThreshold)
            return false;

        var forwardBias = Vector3.Dot(transform.forward, desiredDirection);
        var upwardedBias = Vector3.Dot(transform.up, desiredDirection);
        var forwardAngle = Vector3.Angle(transform.forward, desiredDirection);

        return forwardBias > upwardedBias                       // 주먹 기준, 정면방향 이동이 윗방향 이동보다 큰 지
                && forwardAngle <= punchForwardAngleThreshold;  // 주먹의 정면 방향으로 내지르는지
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
