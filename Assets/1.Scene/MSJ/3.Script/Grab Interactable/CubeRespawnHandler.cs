using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRespawnHandler : MonoBehaviour
{
    public Rigidbody cubeRigidbody;

    [field: SerializeField] public bool IsUsed { get; set; } = false;
    [field: SerializeField] public CubeSlot AssignedSlot { get; set; }

    private void Awake()
    {
        TryGetComponent(out cubeRigidbody);
    }

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
            AssignedSlot.IsEmpty = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SlotZone"))
        {
            AssignedSlot.IsEmpty = true;
        }
    }

    public void ToggleRigidbodyKinematic(bool isKinematic)
    {
        cubeRigidbody.isKinematic = isKinematic;
    }
}
