using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VRHandController : NetworkBehaviour
{
    public SkinnedMeshRenderer NonPhysicsLeftHand;
    public SkinnedMeshRenderer NonPhysicsRightHand;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            NonPhysicsLeftHand.enabled = false;
            NonPhysicsRightHand.enabled = false;
        }
    }
}
