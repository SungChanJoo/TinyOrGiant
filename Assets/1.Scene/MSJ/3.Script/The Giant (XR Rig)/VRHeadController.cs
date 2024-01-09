using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem.XR;

public class VRHeadController : NetworkBehaviour
{
    public GameObject XRMainCameraObject;
    private Camera XRMainCamera;
    public Transform XRHead;

    [Header("Head Aim")]
    public float maxDistance = 100f;
    public Vector3 currentAimPoint;
    public float aimPointGizmoRadius = .5f;
    public bool isGroundHit = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        XRMainCamera = XRMainCameraObject.GetComponent<Camera>();
        if (!isLocalPlayer)
        {
            XRMainCamera.enabled = false;
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

        var screenCenterPos = new Vector3(XRMainCamera.pixelWidth / 2, XRMainCamera.pixelHeight / 2, 0);
        var ray = XRMainCamera.ScreenPointToRay(screenCenterPos);
        isGroundHit = Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance/*, LayerMask.NameToLayer("Ground")*/) && hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground");
        currentAimPoint = isGroundHit ? hitInfo.point : XRMainCamera.transform.position + XRMainCamera.transform.forward * maxDistance;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(XRMainCamera.transform.position, XRMainCamera.transform.position + XRMainCamera.transform.forward * maxDistance);

        Gizmos.DrawSphere(currentAimPoint, aimPointGizmoRadius);
    }
}
