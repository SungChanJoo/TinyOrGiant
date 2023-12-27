using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerExt : NetworkManager
{
    GameObject leftHandPresencePhysicsPrefab;
    GameObject rightHandPresencePhysicsPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        leftHandPresencePhysicsPrefab = spawnPrefabs[0];
        rightHandPresencePhysicsPrefab = spawnPrefabs[1];
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        var leftHandPresenceObject = Instantiate(leftHandPresencePhysicsPrefab);
        var rightHandPresenceObject = Instantiate(rightHandPresencePhysicsPrefab);

        NetworkServer.Spawn(leftHandPresenceObject);
        NetworkServer.Spawn(rightHandPresenceObject);
    }
}
