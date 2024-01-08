using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem.XR;

public class VRHeadController : NetworkBehaviour
{
    public GameObject XRMainCameraObject;
    public Transform XRHead;

    [Header("Head Aim")]
    public float maxDistance = 100f;

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

        if(Physics.Raycast(XRHead.position, XRHead.forward, out RaycastHit hitInfo, maxDistance))
        {
            Debug.Log($"Ray Hit : {hitInfo.rigidbody.gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(XRHead.position, XRHead.position + XRHead.forward * maxDistance);
    }
}
