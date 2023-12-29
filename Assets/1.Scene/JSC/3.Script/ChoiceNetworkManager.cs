using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class ChoiceNetworkManager : NetworkManager
{
    public Vector3 PlayerPos = new Vector3(0, 5f, 0);
    public void ReplacePlayer(NetworkConnectionToClient conn, GameObject newPrefab)
    {
        GameObject oldPlayer = conn.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(conn, Instantiate(newPrefab, PlayerPos, Quaternion.identity), true);

        Destroy(oldPlayer, 0.1f);
    }

}

