using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryControlPointController : MonoBehaviour
{
    public Transform mapCenter;

    [Range(1f, 100f)] public float frequency = 5f;
    [Range(0f, 10f)] public float moveDistance = 5f;
    [Range(0f, 10f)] public float moveSpeed = 5f;

    Vector3 originPosition;
    Vector3 moveDirection;

    private void Awake()
    {
        originPosition = transform.position;
        moveDirection = (transform.position - mapCenter.position).normalized;
    }

    private IEnumerator Start()
    {
        // Random bounce
        float elapsedTime = 0f;
        while (true)
        {
            elapsedTime += Time.deltaTime;

            Vector3 offset = Mathf.Sin(elapsedTime * frequency / Mathf.PI * 2) * moveDistance * moveDirection;
            Vector3 newPosition = originPosition + offset;
            transform.position = newPosition;

            yield return null;
        }
    }
}
