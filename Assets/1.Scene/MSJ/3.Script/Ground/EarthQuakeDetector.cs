using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthQuakeDetector : MonoBehaviour
{
    // �浹 �� rigidbody.velocity üũ �� filter
    [Range(50f, 500f)] public float velocityThreshold = 50f;

    // ���� �ð� ������ �ѹ��� �浹ó�� �ǵ��� filter
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
            //Debug.Log("EarthQuake !!");
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
