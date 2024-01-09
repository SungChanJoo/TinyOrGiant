using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EarthQuakeDetector : NetworkBehaviour
{
    public GameObject EarthQuakeImpactPrefab;

    public bool isEarthquakePoseOnly = true;

    // 충돌 시 rigidbody.velocity 체크 후 filter
    [Range(0f, 500f)] public float velocityThreshold = 50f;

    // 일정 시간 내에는 한번만 충돌처리 되도록 filter
    [Range(.1f, 10f)] public float leftHandCoolDown = .5f;
    [Range(.1f, 10f)] public float rightHandCoolDown = .5f;

    public float curLeftCoolDown = 0f;
    public float curRightCoolDown = 0f;

    private IEnumerator Start()
    {
        while (true)
        {
            curLeftCoolDown = Mathf.Max(0, curLeftCoolDown - Time.deltaTime);
            curRightCoolDown = Mathf.Max(0, curRightCoolDown - Time.deltaTime);
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        var isLeftHandHit = IsLeftHandHit(collision);
        var isRightHandHit = IsRightHandHit(collision);
        var isEarthQuake = isLeftHandHit || isRightHandHit;

        if (isLeftHandHit)
        {
            curLeftCoolDown = leftHandCoolDown;
        }

        if (isRightHandHit)
        {
            curRightCoolDown = rightHandCoolDown;
        }

        if (isEarthQuake)
        {
            var handPhysics = collision.gameObject.GetComponent<HandPresencePhysics>();
            var handController = handPhysics.controller;
            handController.SendHapticImpulse(handPhysics.earthquake_hapticIntensity, handPhysics.earthquake_hapticDuration);

            var position = collision.collider.ClosestPoint(transform.position);

            var rotationEuler = collision.gameObject.transform.rotation.eulerAngles;
            Vector3 groundedAngle = new Vector3(0, rotationEuler.y, 0);

            var earthQuakeObj = Instantiate(EarthQuakeImpactPrefab, position, Quaternion.Euler(groundedAngle));
            NetworkServer.Spawn(earthQuakeObj);
        }
    }
    private bool IsLeftHandHit(Collision collision)
    {
        var isPhysicalLeftHand = collision.gameObject.layer == LayerMask.NameToLayer("Left Hand Physics");

        if (!isPhysicalLeftHand || curLeftCoolDown > 0) return false;

        var handPhysics = collision.gameObject.GetComponent<HandPresencePhysics>();
        var isValidPose = isEarthquakePoseOnly ? handPhysics.isEarthquakePose : handPhysics.isFistPose;
        var velocity = collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        return isValidPose && velocity >= velocityThreshold;
    }
        
    private bool IsRightHandHit(Collision collision)
    {
        var isPhysicalRightHand = collision.gameObject.layer == LayerMask.NameToLayer("Right Hand Physics");

        if (!isPhysicalRightHand || curRightCoolDown > 0) return false;

        var handPhysics = collision.gameObject.GetComponent<HandPresencePhysics>();
        var isValidPose = isEarthquakePoseOnly ? handPhysics.isEarthquakePose : handPhysics.isFistPose;
        var velocity = collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        return isValidPose && velocity >= velocityThreshold;
    }
}
