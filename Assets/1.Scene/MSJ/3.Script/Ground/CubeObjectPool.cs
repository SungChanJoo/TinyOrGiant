using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RespawnType
{
    Random = 0,
    RoundRobin = 1,
}

public class CubeObjectPool : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> cubePrefabList = new List<GameObject>();

    [Header("Initial Spawn")]
    public List<CubeSlot> slotList = new List<CubeSlot>();
    public List<GameObject> spawnedCubeList = new List<GameObject>();

    [Header("Respawn")]
    public int currentRepawnCount = 0;
    public RespawnType respawnType = RespawnType.Random;
    public int maxRespawnCount = 20;
    public float respawnInterval = 1f;

    private void Start()
    {
        InitialSpawn();
        StartCoroutine(ReadyToRespawn());
    }

    private void InitialSpawn()
    {
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
            var cubeRespawnHandler = cube.GetComponent<CubeRespawnHandler>();
            cubeRespawnHandler.ToggleRigidbodyKinematic(false);
            cubeRespawnHandler.AssignedSlot = slotList[slotIndex];

            cube.transform.SetPositionAndRotation(
                slotList[slotIndex].transform.position,
                slotList[slotIndex].transform.rotation);

            spawnedCubeList.Add(cube);
        }
    }

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
                if (!slot.IsEmpty) continue;

                emptySlot = slot;
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
            var cubeRespawnHandler = inactiveCube.GetComponent<CubeRespawnHandler>();
            cubeRespawnHandler.ToggleRigidbodyKinematic(false);
            cubeRespawnHandler.IsUsed = false;
            inactiveCube.transform.position = cubeRespawnHandler.AssignedSlot.transform.position;
            inactiveCube.transform.rotation = cubeRespawnHandler.AssignedSlot.transform.rotation;

            // Activate cube
            inactiveCube.SetActive(true);

            elapsedTime = 0f;
            currentRepawnCount++;
        }
    }
}
