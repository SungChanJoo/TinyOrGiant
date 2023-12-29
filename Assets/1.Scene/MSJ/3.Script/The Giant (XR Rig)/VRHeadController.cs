using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem.XR;

public class VRHeadController : NetworkBehaviour
{
    public GameObject XRMainCameraObject;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            XRMainCameraObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        transform.SetPositionAndRotation(
            XRMainCameraObject.transform.position, 
            XRMainCameraObject.transform.rotation);
    }
}
