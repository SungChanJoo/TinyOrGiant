using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagicalPunchProjectileController : MonoBehaviour
{
    [Range(0f, 500f)] public float speed = 10f;
    [Range(0f, 10f)] public float lifeTime = 3f;
    Rigidbody rigidbody;

    private void Awake()
    {
        TryGetComponent(out rigidbody);
    }

    private IEnumerator Start()
    {
        rigidbody.velocity = transform.forward * speed;
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
