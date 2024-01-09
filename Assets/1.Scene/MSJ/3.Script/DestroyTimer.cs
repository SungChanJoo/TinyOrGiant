using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DestroyTimer : NetworkBehaviour
{
    public float delayTime = 2f;

    void Start()
    {
        //Destroy(gameObject, delayTime);
    }
}
