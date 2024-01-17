using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DealDamage : NetworkBehaviour
{
    public float defaultDamage = 1f;
    public LayerMask targetLayer;

    private ParticleSystem ps;
    private void Awake()
    {
        TryGetComponent(out ps);
    }

    private IEnumerator Start()
    {
        if (ps == null) yield break;

        while (true)
        {
            yield return null;

            var collider = GameObject.Find("Hit Box").GetComponent<Collider>();
            if (collider != null)
            {
                ps.trigger.AddCollider(collider);
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DealDmg(other);
    }

    private void DealDmg(Collider other)
    {
        if (((uint)(1 << other.gameObject.layer) & (uint)targetLayer.value) > 0)
        {
            var targetHealth = other.gameObject.GetComponent<Health>();

            if (targetHealth == null) return;
            targetHealth.CmdTakeDamage(defaultDamage);
            if (gameObject.CompareTag("Bullet"))
            {
                Destroy(gameObject, 1.5f);
            }
        }
    }

    private void OnParticleTrigger()
    {
        // particles
        List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();
        ParticleSystem.ColliderData colliderData;

        // get
        int numInside = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside, out colliderData);

        // iterate through the particles which entered the trigger and make them red
        for (int i = 0; i < numInside; i++)
        {
            ParticleSystem.Particle p = inside[i];
            var collider = colliderData.GetCollider(i, 0);
            DealDmg(collider as Collider);
        }
    }
}
