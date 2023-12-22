using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{ 
    Idle,
    Run,
    Jump,
    Attack
}


public class PCPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float JumpPower = 30f;
    public float Accelation = 10f;
    public float Deccelation = -10f;
    public float velPower = 1.001f;
    public PlayerState state;

    
    private Vector3 _moveDirection = Vector3.zero;

    public Rigidbody rb;
    public PlayerControls input;

    [Header("Player Grounded")]
    public bool isGround;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    private void Awake()
    {
        input = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        state = PlayerState.Idle;
        isGround = true;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Land.Move.performed += OnMovemnetPerformed;
        input.Land.Move.started += OnMovemnetStarted;
        input.Land.Move.canceled += OnMovemnetCancelled;

        input.Land.Jump.performed += Jump;

    }

    private void OnDisable()
    {
        input.Disable();
        input.Land.Move.performed -= OnMovemnetPerformed;
        input.Land.Move.started -= OnMovemnetStarted;
        input.Land.Move.canceled -= OnMovemnetCancelled;

        input.Land.Jump.performed -= Jump;

    }


    private void FixedUpdate()
    {


        GroundedCheck();

        #region Run
        float targetSpeed = _moveDirection.magnitude * MoveSpeed;
        //float speedDif = targetSpeed - rb.velocity.magnitude;
        //if (rb.velocity.x == 0 && rb.velocity.z == 0) return;
        float speedDif = targetSpeed - Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Accelation : Deccelation;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        Debug.Log($"targetSpeed : {targetSpeed}" +
                $"speedDif : {speedDif}" +
                $"<color=red>currentSpeed : {Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z)}</color>" +
                $"movement : {movement}");

        if ((state == PlayerState.Idle || state == PlayerState.Run) )
        {
            if(rb.velocity.y < 0)
            {
                state = PlayerState.Jump;
            }
            rb.AddForce(_moveDirection * movement, ForceMode.Force);
            Debug.Log(_moveDirection * movement);
        }
        #endregion


        //움직이는 상태일 때
        /*        if (!_moveDirection.Equals(Vector3.zero) && state == PlayerState.Run)
                {
                    rb.AddForce(_moveDirection * MoveSpeed, ForceMode.Force);
                    //rb.velocity = _moveDirection * MoveSpeed;
                    Debug.Log(_moveDirection);

                }*/
        /*if (rb.velocity.y < 0)
{

    Debug.DrawRay(rb.position, Vector3.down * 3f, new Color(1,0,0));
    if (Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, 3f))
    {
        Debug.Log(hit.distance);

        if (hit.distance < 1f)
        {
            state = PlayerState.Idle;
            isGround = true;
        }
    }

}*/
    }

    private void OnMovemnetStarted(InputAction.CallbackContext value)
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        Debug.Log("OnMovemnetStarted");
    }
    private void OnMovemnetPerformed(InputAction.CallbackContext value)
    {
        state = PlayerState.Run;
        _moveDirection = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);
        Debug.Log("OnMovemnetPerformed");
    }
    private void OnMovemnetCancelled(InputAction.CallbackContext value)
    {
        //점프상태가 아니고 땅에 닿아 있을때
        if(state != PlayerState.Jump && isGround)
        {
        }
        state = PlayerState.Idle;
        _moveDirection = Vector3.zero;
        //StartCoroutine(DecreaseMoveSpeed_co());
    }


    private void Jump(InputAction.CallbackContext obj)
    {
        //todo 1220땅에 닿았을때만 가능하게 조건 추가해줘
        if(isGround)
        {
            isGround = false;
            state = PlayerState.Jump;
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }

    }
    private void GroundedCheck()
    {
        if (rb.velocity.y < 0)
        {
            //_moveDirection = Vector3.zero;

            Debug.Log("떨어지는 중");
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
            isGround = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
            if (isGround)
            {
                state = PlayerState.Run;
            }
        }

        // update animator if using character
        /*        if (_hasAnimator)
                {
                    _animator.SetBool(_animIDGrounded, Grounded);
                }*/
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                                    transform.position.z);
        Gizmos.DrawSphere(spherePosition, GroundedRadius);
    }
}
