using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationManager : MonoBehaviour
{
    public TeleportationProvider teleportationProvider;
    TeleportationAnchor[] teleportationAnchors;

    private void Start()
    {
        teleportationAnchors = FindObjectsOfType<TeleportationAnchor>();

        foreach (var anchor in teleportationAnchors)
        {
            anchor.teleportationProvider = teleportationProvider;
        }
    }
}
