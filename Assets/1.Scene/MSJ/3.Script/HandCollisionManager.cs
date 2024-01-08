using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionManager : MonoBehaviour
{
    public List<LayerMask> hittableLayers = new List<LayerMask>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHittableLayer(other.gameObject.layer)) return;

        Debug.Log("Player hit by physical hand");
    }

    private bool IsHittableLayer(LayerMask layerMask)
    {
        foreach (var layer in hittableLayers)
        {
            if (layer == 1 << layerMask) return true;
        }

        return false;
    }
}
