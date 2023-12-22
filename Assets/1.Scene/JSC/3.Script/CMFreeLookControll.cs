using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMFreeLookControll : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook _freeLook;
    [SerializeField] PCPlayerController _player;
    // Start is called before the first frame update
    private void Awake()
    {
        _freeLook = this.GetComponent<CinemachineFreeLook>();
        _player = GameObject.FindObjectOfType<PCPlayerController>();

    }
    void Start()
    {
        _freeLook.Follow = _player.transform;
        _freeLook.LookAt = _player.transform;
    }
}
