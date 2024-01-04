using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MagicalPunchDetector : NetworkBehaviour
{
    public GameObject MagicalImpactPrefab;

    public Vector3 offset = new Vector3(0, 0.01f, 0);

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.gameObject.layer != LayerMask.NameToLayer("Magical Punch")) return;

        CreateMagicalImpact(other.ClosestPoint(transform.position));
    }

    private void CreateMagicalImpact(Vector3 position)
    {
        var magicalImpcatObj = Instantiate(MagicalImpactPrefab, position + offset, Quaternion.identity);
        NetworkServer.Spawn(magicalImpcatObj);
    }
}
