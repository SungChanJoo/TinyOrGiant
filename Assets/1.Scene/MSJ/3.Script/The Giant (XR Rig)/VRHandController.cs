using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VRHandController : NetworkBehaviour
{
    public GameObject NonPhysicsLeftHand;
    public GameObject NonPhysicsRightHand;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            NonPhysicsLeftHand.SetActive(false);
            NonPhysicsRightHand.SetActive(false);
        }
    }
}
