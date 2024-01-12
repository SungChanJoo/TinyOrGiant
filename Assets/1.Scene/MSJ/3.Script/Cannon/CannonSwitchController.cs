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

        // Activate Cannon
        if ((canTurnOn & opponent) > 0)
        {
            // 중복실행 방지
            if (isTurnedOn) return;

            cannonController.ActivateCannon();
        }

        // Deactivate Cannon
        if ((canTurnOff & opponent) > 0)
        {
            // 중복실행 방지
            if (!isTurnedOn) return;

            cannonController.DeactivateCannon();
        }
    }

    public void SetSwitchState(bool isTurnOn)
	{
        isTurnedOn = isTurnOn;
        renderer.material = isTurnedOn ? turnOnMaterial : turnOffMaterial;
	}
}
