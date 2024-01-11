using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DealDamage : NetworkBehaviour
{
    public float defaultDamage = 1f;
    public LayerMask targetLayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != targetLayer) return;

        var targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth == null) return;
        CmdTakeDamage(collision, defaultDamage);
    }
    [Command]
    public void CmdTakeDamage(Collision collision, float damage)
    {
        RpcTakeDamage(collision, damage);
    }
    [ClientRpc]
    public void RpcTakeDamage(Collision collision, float damage)
    {
        var targetHealth = collision.gameObject.GetComponent<Health>();
        targetHealth.TakeDamage(damage);
    }
}
