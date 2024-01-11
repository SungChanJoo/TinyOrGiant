using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class ChoiceNetworkManager : NetworkManager
{
    public Transform[] PlayerSpawnPos;
    private int _PlayerCount=0;
    public void ReplacePlayer(NetworkConnectionToClient conn, GameObject newPrefab)
    {
        GameObject oldPlayer = conn.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(conn, Instantiate(newPrefab, PlayerSpawnPos[_PlayerCount].position, Quaternion.identity), true);
        _PlayerCount++;
        if (_PlayerCount >= PlayerSpawnPos.Length)
            _PlayerCount = 0;

        Destroy(oldPlayer, 0.1f);
    }
}

