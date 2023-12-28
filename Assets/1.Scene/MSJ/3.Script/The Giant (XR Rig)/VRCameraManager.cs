using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.XR.CoreUtils;

public class VRCameraManager : NetworkBehaviour
{
    public XROrigin xrOrigin;
    public Transform cameraOffset;

    public GameObject headPhysicsPrefab;
    GameObject headPhysicsObject;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Camera.main.transform.SetParent(cameraOffset);
        Camera.main.transform.localPosition = Vector3.zero;

        xrOrigin.Camera = Camera.main;
    }

    private void Start()
    {
        headPhysicsObject = Instantiate(headPhysicsPrefab, Camera.main.transform);
    }
}
