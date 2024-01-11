using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    public float defaultDamage = 1f;
    public LayerMask targetLayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != targetLayer) return;

        var targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth == null) return;

        targetHealth.TakeDamage(defaultDamage);
    }
}
