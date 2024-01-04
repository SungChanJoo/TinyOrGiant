using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EarthQuakeDetector : NetworkBehaviour
{
    public GameObject EarthQuakeImpactPrefab;

    // 충돌 시 rigidbody.velocity 체크 후 filter
    [Range(50f, 500f)] public float velocityThreshold = 50f;

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
        var isLeftHandHit = IsLeftHandHit(collision);
        var isRightHandHit = IsRightHandHit(collision);
        var isEarthQuake = isLeftHandHit || isRightHandHit;

        if (isLeftHandHit)
        {
            Debug.Log($"Left Hand !!");
            curLeftCoolDown = leftHandCoolDown;
        }

        if (isRightHandHit)
        {
            Debug.Log($"RightHand !!");
            curRightCoolDown = rightHandCoolDown;
        }

        if (isEarthQuake)
        {
            var position = collision.collider.ClosestPoint(transform.position);
            var rotation = collision.gameObject.transform.rotation;

            var earthQuakeObj = Instantiate(EarthQuakeImpactPrefab, position, rotation);
            NetworkServer.Spawn(earthQuakeObj);
        }
    }

    private bool IsLeftHandHit(Collision collision)
    {
        var isPhysicalLeftHand = collision.gameObject.layer == LayerMask.NameToLayer("Left Hand Physics");

        if (!isPhysicalLeftHand) return false;

        var isFist = collision.gameObject.GetComponent<HandPresencePhysics>().isFist;
        var velocity = collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        return isFist                               // Check Fisted
                && velocity >= velocityThreshold    // Check velocity
                && curLeftCoolDown == 0;            // Check Cool Down
    }

    private bool IsRightHandHit(Collision collision)
    {
        var isPhysicalRightHand = collision.gameObject.layer == LayerMask.NameToLayer("Right Hand Physics");

        if (!isPhysicalRightHand) return false;

        var isFist = collision.gameObject.GetComponent<HandPresencePhysics>().isFist;
        var velocity = collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        return isFist                               // Check Fisted
                && velocity >= velocityThreshold    // Check velocity
                && curRightCoolDown == 0;           // Check Cool Down
    }
}
