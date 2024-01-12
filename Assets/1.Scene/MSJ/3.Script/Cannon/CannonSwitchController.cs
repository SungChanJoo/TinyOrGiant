using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonSwitchController : MonoBehaviour
{
    public bool isTurnedOn = false;
    public CannonController cannonController;

    [Header("Turn On")]
    public Material turnOnMaterial;
    public LayerMask targetToTurnOn;

    [Header("Turn Off")]
    public Material turnOffMaterial;
    public LayerMask targetToTurnOff;
    
    private MeshRenderer renderer;

    private void Awake()
    {
        TryGetComponent(out renderer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var opponent = (uint)(1 << collision.gameObject.layer);
        var canTurnOn = (uint)targetToTurnOn.value;
        var canTurnOff = (uint)targetToTurnOff.value;

        // Turn On
        if ((canTurnOn & opponent) > 0)
        {
            isTurnedOn = !isTurnedOn;
            renderer.material = isTurnedOn ? turnOnMaterial : turnOffMaterial;
            
            if (isTurnedOn) cannonController.ActivateCannon();
            else cannonController.DeactivateCannon();
        }

        // Turn Off
        if ((canTurnOff & opponent) > 0)
        {
            isTurnedOn = false;
            renderer.material = turnOffMaterial;
        }
    }
}
