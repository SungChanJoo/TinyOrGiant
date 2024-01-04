using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    public float delayTime = 2f;

    void Start()
    {
        StartCoroutine(Destroy(delayTime));
    }

    private IEnumerator Destroy(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(gameObject);
    }
}
