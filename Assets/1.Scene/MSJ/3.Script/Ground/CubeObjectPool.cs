using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum RespawnType
{
    Random = 0,
    RoundRobin = 1,
}

public class CubeObjectPool : NetworkBehaviour
{
    [Header("Pre Defined")]
    public List<GameObject> cubePrefabList = new List<GameObject>();
    public List<GameObject> slotList = new List<GameObject>();
    public List<GameObject> spawnedCubeList = new List<GameObject>();

    [Header("Respawn")]
    public int currentRepawnCount = 0;
    public RespawnType respawnType = RespawnType.Random;
    public int maxRespawnCount = 20;
    public float respawnInterval = 1f;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        InitialSpawn();
        StartCoroutine(ReadyToRespawn());
    }

    [Server]
    private void InitialSpawn()
    {
        if (isClientOnly && !isLocalPlayer) return;

        int slotIndex = -1;
        int prefabIndex = -1;

        for (int i = 0; i < slotList.Count; i++)
        {
            slotIndex++;

            switch (respawnType)
            {
                case RespawnType.Random:
                    prefabIndex = Random.Range(0, cubePrefabList.Count);
                    break;
                case RespawnType.RoundRobin:
                    prefabIndex = ++prefabIndex % cubePrefabList.Count;
                    break;
            }

            var cube = Instantiate(cubePrefabList[prefabIndex]);

            var cubeRespawnHandler = cube.GetComponent<CubeHandler>();
            cubeRespawnHandler.ToggleRigidbodyKinematic(false);
            cubeRespawnHandler.SetCubeUsed(false);

            cubeRespawnHandler.AssignCubeSlot(slotList[slotIndex]);

            var cubeSlotTransform = slotList[slotIndex].transform;
            cube.transform.SetPositionAndRotation(cubeSlotTransform.position, cubeSlotTransform.rotation);

            if (isServer) NetworkServer.Spawn(cube);
            spawnedCubeList.Add(cube);
        }
    }

    [Server]
    private IEnumerator ReadyToRespawn()
    {
        float elapsedTime = 0f;

        while (currentRepawnCount < maxRespawnCount)
        {
            yield return null;

            // Delay for respawnInterval
            if (elapsedTime < respawnInterval)
            {
                elapsedTime += Time.deltaTime;
                continue;
            }

            // Find empty slot
            CubeSlot emptySlot = null;
            foreach (var slot in slotList)
            {
                if (!slot.GetComponent<CubeSlot>().IsEmpty) continue;

                emptySlot = slot.GetComponent<CubeSlot>();
                break;
            }

            // Find inactive cube
            GameObject inactiveCube = null;
            foreach (var cube in spawnedCubeList)
            {
                if (cube.activeSelf) continue;

                inactiveCube = cube;
                break;
            }

            if (emptySlot == null || inactiveCube == null) continue;

            // Respawn cube at slot
            var cubeRespawnHandler = inactiveCube.GetComponent<CubeHandler>();
            cubeRespawnHandler.ToggleRigidbodyKinematic(false);
            cubeRespawnHandler.SetCubeUsed(false);

            var cubeSlotTransform = cubeRespawnHandler.AssignedSlot.transform;
            inactiveCube.transform.SetPositionAndRotation(cubeSlotTransform.position, cubeSlotTransform.rotation);

            // Activate cube
            inactiveCube.SetActive(true);

            elapsedTime = 0f;
            currentRepawnCount++;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        InitialSync();
    }

    [Client]
    private void InitialSync()
    {
        int index = 0;

        foreach (var spawnedCube in spawnedCubeList)
        {
            var cubeRespawnHandler = spawnedCube.GetComponent<CubeHandler>();
            cubeRespawnHandler.AssignedSlot = slotList[index++].GetComponent<CubeSlot>();
        }
    }
}
