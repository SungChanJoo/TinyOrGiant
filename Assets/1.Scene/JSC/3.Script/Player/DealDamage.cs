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
        CmdTakeDamage(targetHealth , defaultDamage);
    }
    [Command]
    public void CmdTakeDamage(Health targetHealth, float damage)
    {
        RpcTakeDamage(targetHealth, damage);
    }
    [ClientRpc]
    public void RpcTakeDamage(Health targetHealth, float damage)
    {
        targetHealth.TakeDamage(damage);

    }
}
