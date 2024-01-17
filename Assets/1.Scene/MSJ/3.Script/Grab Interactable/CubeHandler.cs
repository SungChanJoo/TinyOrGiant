using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CubeHandler : NetworkBehaviour
{
    Rigidbody cubeRigidbody;

    [SyncVar]
    public bool IsUsed = false;
    public CubeSlot AssignedSlot;

    private void Awake()
    {
        TryGetComponent(out cubeRigidbody);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
        {
            ToggleRigidbodyKinematic(true);
            gameObject.SetActive(false);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("SlotZone")
            && !IsUsed)
        {
            ToggleRigidbodyKinematic(true);
            SetCubeSlotEmpty(false);
        }
    }

    [Server]
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SlotZone"))
        {
            SetCubeSlotEmpty(true);
        }
    }

    #region Manual Sync
    [Server]
    public void SetCubeSlotEmpty(bool isEmpty)
    {
        AssignedSlot.IsEmpty = isEmpty;
    }

    [Server]
    public void SetCubeUsed(bool isUsed)
    {
        IsUsed = isUsed;
    }

    [Server]
    public void AssignCubeSlot(GameObject slotObj)
    {
        AssignedSlot = slotObj.GetComponent<CubeSlot>();
    }
    #endregion

    [Server]
    public void ToggleRigidbodyKinematic(bool isKinematic)
    {
        cubeRigidbody.isKinematic = isKinematic;
        RPCToggleRigidbodyKinematic(isKinematic);
    }

    [ClientRpc]
    public void RPCToggleRigidbodyKinematic(bool isKinematic)
    {
        cubeRigidbody.isKinematic = isKinematic;
    }
}
