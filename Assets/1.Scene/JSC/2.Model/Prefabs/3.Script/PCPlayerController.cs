using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PCPlayerController : MonoBehaviour
{

    public float MoveSpeed = 3f;
    public float JumpPower = 30f;
    float _gravityValue = -9.81f;

    Vector3 _playerVelocity;

    private Vector3 _moveDirection = Vector3.zero;

    public Rigidbody rb;
    public PlayerControls input;

    private void Awake()
    {
        input = new PlayerControls();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Land.Move.performed += OnMovemnetPerformed;
        input.Land.Move.canceled += OnMovemnetCancelled;

        input.Land.Jump.performed += Jump;

    }

    private void OnDisable()
    {
        input.Disable();
        input.Land.Move.performed -= OnMovemnetPerformed;
        input.Land.Move.canceled -= OnMovemnetCancelled;

        input.Land.Jump.performed -= Jump;

    }

    private void Update()
    {


    }
    private void FixedUpdate()
    {
        //if (!isLocalPlayer) return;
        //Debug.Log(Controller.isGrounded);
        if(!_moveDirection.Equals(Vector3.zero))
        rb.velocity = _moveDirection * MoveSpeed;

    }

    private void OnMovemnetPerformed(InputAction.CallbackContext value)
    {

        _moveDirection = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);

    }

    private void OnMovemnetCancelled(InputAction.CallbackContext value)
    {
        _moveDirection = Vector3.zero;
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        //todo 1220땅에 닿았을때만 가능하게 조건 추가해줘
        rb.AddForce(Vector3.up * JumpPower);
    }
}
