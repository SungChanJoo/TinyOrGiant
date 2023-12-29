using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HeadPhysicsController : NetworkBehaviour
{
    public GameObject XRMainCameraObject;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            XRMainCameraObject.GetComponent<Camera>().enabled = false;
            XRMainCameraObject.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void Update()
    {
        transform.SetPositionAndRotation(
            XRMainCameraObject.transform.position, 
            XRMainCameraObject.transform.rotation);
    }
}
