using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagicalPunchProjectileController : MonoBehaviour
{
    public float lifeTime = 3f;
    public float destoryDelay = .5f;
    [Range(0f, 500f)] public float speed = 10f;

    //[Range(0f, 10f)] public float lifeTime = 3f;
    private Rigidbody rigidbody;
    private VRHeadController headController;

    private void Awake()
    {
        TryGetComponent(out rigidbody);
        headController = FindObjectOfType<VRHeadController>();
    }

    private Vector3 targetPos;

    public void StartMove()
    {
        targetPos = headController.currentAimPoint;
        StartCoroutine(UpdateRotation());
        rigidbody.velocity = (targetPos - transform.position).normalized * speed;
    }
    
    private IEnumerator UpdateRotation()
    {
        transform.forward = (targetPos - transform.position).normalized;

        float elapsedTime = 0f;
        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;
            transform.forward = (targetPos - transform.position).normalized;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject, destoryDelay);
    }
}
