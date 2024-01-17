using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShellController : MonoBehaviour
{
    public GameObject target = null;
    [Range(1f, 100f)]
    public float speed = 10f;

    [Range(1f, 100f)]
    public float distanceThreshold = 1f;
    [Range(0.01f, 100f)]
    public float gravityModifier = .01f;

    private Rigidbody rigidbody;

    private void Awake()
    {
        TryGetComponent(out rigidbody);
    }

    private IEnumerator Start()
    {
        // Find VRHead game object
        while (target == null)
        {
            yield return null;
            target = GameObject.FindGameObjectWithTag("PhysicsHead");
        }
        var targetPosition = target.transform.position;

        // Move straight to VRHead
        var direction = (targetPosition - transform.position).normalized;
        var velocity = direction * speed;
        rigidbody.velocity = velocity;

        // Wait to reach target position
        while (Vector3.Distance(transform.position, targetPosition) > distanceThreshold)
        {
            yield return null;
        }

        // Apply gravity
        while (true)
        {
            yield return null;
            rigidbody.velocity += Vector3.down * gravityModifier;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DeadZone")
            || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
