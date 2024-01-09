using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCanvasManager : MonoBehaviour
{
    private Canvas VRCanvas;
    public GameObject aimReticle;
    public Vector3 aimReticleOffset;

    private VRHeadController VRHeadController;

    private void Awake()
    {
        TryGetComponent(out VRCanvas);
    }

    private IEnumerator Start()
    {
        while (VRCanvas != null && VRCanvas.worldCamera == null)
        {
            GameObject XRMainCamera = GameObject.FindGameObjectWithTag("XRMainCamera");
            if (XRMainCamera != null) VRCanvas.worldCamera = XRMainCamera.GetComponent<Camera>();
            yield return null;
        }

        while (VRHeadController == null)
        {
            VRHeadController = FindObjectOfType<VRHeadController>();
            yield return null;
        }
    }

    private void Update()
    {
        if (VRHeadController == null) return;

        aimReticle.transform.position = VRHeadController.currentAimPoint + aimReticleOffset;
        aimReticle.transform.up = Vector3.forward;
        aimReticle.SetActive(VRHeadController.isGroundHit);
    }
}
