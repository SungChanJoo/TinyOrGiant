using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPositionChecker : MonoBehaviour
{
    [field: Header("Releative position to head")]
    [field: SerializeField] public bool IsOverHead { get; private set; } = false;

    [Range(0f, 50f)] public float altitudeOffset = 2f;
    public Transform headTransform;
    public XRBaseInteractor teleportRayInteractor;

    private IEnumerator Start()
    {
        // Check relative altitude of hand to head
        while (true)
        {
            var handAltitude = transform.position.y;
            var headAltitude = headTransform.position.y + altitudeOffset;

            IsOverHead = handAltitude > headAltitude;
            ToggleInteractor(IsOverHead);

            yield return null;
        }
    }

    private void ToggleInteractor(bool isActivate)
    {
        teleportRayInteractor.enabled = isActivate;
    }
}
