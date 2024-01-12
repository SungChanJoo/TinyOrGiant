using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DealDamage : NetworkBehaviour
{
    public float defaultDamage = 1f;
    public LayerMask targetLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (((uint)(1 << other.gameObject.layer) & (uint)targetLayer.value) > 0)
        {
            var targetHealth = other.gameObject.GetComponent<Health>();

            if (targetHealth == null) return;
            targetHealth.CmdTakeDamage(defaultDamage);

            Destroy(gameObject, 1.5f);
        }
    }
}
