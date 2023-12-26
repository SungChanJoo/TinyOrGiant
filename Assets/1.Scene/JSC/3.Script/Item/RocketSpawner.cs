using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketSpawner : MonoBehaviour
{
    public GameObject Rocket;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRocket_co());
    }
    
    IEnumerator SpawnRocket_co()
    {
        while(true)
        {
            if(!Rocket.activeSelf)
            {
                Rocket.SetActive(true);
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
