using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBullet : MonoBehaviour
{
    Rigidbody _rb;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float offest_y = 3f;
    [SerializeField] private float _graph = 0.3f;
    private bool isFire = false;
/*    [SerializeField]
    private float _trailRate = 200;
    [SerializeField]
    private GameObject _smokeTrail;
    [SerializeField]
    private GameObject _explosion;*/

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        PCPlayerController.OnFired += OnRocketFired;
        //Physics.IgnoreLayerCollision(7, 7, true);
    }

    /*    private void OnCollisionEnter(Collision collision)
        {
    *//*        if (collision.collider.CompareTag("Destructable"))
            {
                //Instantiate(_explosion, collision.transform.position, Quaternion.identity);
                Destroy(collision.collider.gameObject);
            }*//*

            //Destroy(gameObject);
        }*/
    private void FixedUpdate()
    {
/*        Vector3 dir = Camera.main.ScreenPointToRay(Input.mousePosition).direction;
        Ray ray = new Ray(transform.position, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, 999f))
        {
            Debug.Log(hit.transform.gameObject.name);
        }*/
        if (_rb.velocity.y <4)
        {
            //앞으로 기울일려면 z축을 +해주면 된다.
            Quaternion targetRotation = new Quaternion();
            targetRotation.eulerAngles = new Vector3( transform.rotation.eulerAngles.x,
                                                      transform.rotation.eulerAngles.y,
                                                      transform.rotation.eulerAngles.z + Mathf.Abs(_rb.velocity.y) * _graph);
            //Debug.Log("eulerAngles" + targetRotation.eulerAngles);
            if(transform.rotation.eulerAngles.z > 180)
            {
                return;
            }
            transform.rotation = targetRotation;
            //transform.rotation = Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z - _rb.velocity.y);
        }
    }

    private void OnRocketFired()
    {
        if (isActiveAndEnabled)
        {
            //_smokeTrail.SetActive(true);
            _rb.isKinematic = false;
            _rb.useGravity = true;

            //Vector3 dir = Camera.main.ScreenPointToRay(Input.mousePosition).direction;
            //Ray ray = new Ray(player.transform.position, dir);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 999f))
            {
                Debug.Log(hit.transform.gameObject.name);
            }
            Vector3 editRayDirection = new Vector3(ray.direction.x, ray.direction.y + offest_y, ray.direction.z);
            _rb.AddForce(editRayDirection * _speed, ForceMode.Impulse);
            isFire = true;
            PCPlayerController.OnFired -= OnRocketFired;

        }
    }
}
