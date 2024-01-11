using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NonePlayer : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        AddPlayer();
    }
    [Command]
    private void AddPlayer()
    {
        GameManager.Instance.PlayerSyncList.Add(gameObject);
    }
}
