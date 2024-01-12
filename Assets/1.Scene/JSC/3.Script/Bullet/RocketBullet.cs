using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RocketBullet : NetworkBehaviour
{
    public Rigidbody Rb;
    [SerializeField] private float _speed = 100f;
    [SerializeField] private float offest_y = 1f;
    [SerializeField] private float _graph = 0.2f;
    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private GameObject RocketLineEffect;
    private bool isFire;

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Rb.isKinematic = true;
        Rb.useGravity = false;
        PCPlayerController.OnFired += OnRocketFired;
        //Physics.IgnoreLayerCollision(7, 7, true);
    }
    private void OnEnable()
    {
    }
    private void FixedUpdate()
    {
        if (Rb.velocity.y <4)
        {
            //앞으로 기울일려면 z축을 +해주면 된다.
            Quaternion targetRotation = new Quaternion();
            targetRotation.eulerAngles = new Vector3( transform.rotation.eulerAngles.x,
                                                      transform.rotation.eulerAngles.y,
                                                      transform.rotation.eulerAngles.z + Mathf.Abs(Rb.velocity.y) * _graph);
            if(transform.rotation.eulerAngles.z > 180)
            {
                return;
            }
            transform.rotation = targetRotation;
        }
    }

    private void OnRocketFired(Vector3 targetVector)
    {
        if (isActiveAndEnabled)
        {
            //_smokeTrail.SetActive(true);
            Rb.isKinematic = false;
            Rb.useGravity = true;

            Vector3 editRayDirection = new Vector3(targetVector.x, targetVector.y + offest_y, targetVector.z);
            Rb.AddForce(editRayDirection * _speed, ForceMode.Impulse);
            isFire = true;
            PCPlayerController.OnFired -= OnRocketFired;
            Destroy(gameObject, 5f);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PhysicsHead"))
        {
            if (isFire)
            {
                //Todo 0108 적에게 데미지주는 메소드 추가해줘
                var Effect = Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
                Destroy(Effect, 2f);
                //RocketLineEffect.GetComponent<ParticleSystem>().loop = false;
                //CmdTakeDamage(1f);

/*                Rb.isKinematic = true;
                Rb.useGravity = false;

                Destroy(gameObject);*/
            }

        }
    }
/*    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(float damage)
    {
        Debug.Log("CmdTakeDamage");
        RpcTakeDamage(damage);
    }
    [ClientRpc]
    public void RpcTakeDamage(float damage)
    {
        Debug.Log("RpcTakeDamage");
        Debug.Log("damage" + damage);
        *//*        var targetHealth = obj.GetComponent<Health>();
                targetHealth.TakeDamage(damage);*//*

    }*/

}
