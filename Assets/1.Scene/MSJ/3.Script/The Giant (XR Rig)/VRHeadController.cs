using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem.XR;

public class VRHeadController : NetworkBehaviour
{
    public GameObject XRMainCameraObject;
    public Transform XRHead;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            XRMainCameraObject.GetComponent<Camera>().enabled = false;
            XRMainCameraObject.GetComponent<AudioListener>().enabled = false;
            XRMainCameraObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        XRHead.SetPositionAndRotation(
            XRMainCameraObject.transform.position, 
            XRMainCameraObject.transform.rotation);
    }
}
