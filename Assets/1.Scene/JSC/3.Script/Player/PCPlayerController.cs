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
    public float Deccelation = 10f;
    public float velPower = 1.001f;
    public float frictionAmount = 5f;
    public float RotationSpeed = 10f;
    public float TurnSmoothTime = 0.1f;
    float _turnSmoothVelocity;
    public PlayerState state;

    private float _playerVelocity;
    private float _targetSpeed;
    private Vector3 _moveDirection = Vector3.zero;

    public Rigidbody rb;
    public PlayerControls input;
    public Transform Cam;

    [Header("Player Grounded")]
    public float GravityScale = 5f;
    public bool isGround;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("ETC")]
    public GameObject RocketPrefab;
    public int RocketCount=0;
    public GameObject RocketParent;
    public Transform[] EquidRocketPos;
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

        input.Land.Fire.performed += Fire;
        input.Land.Fire.canceled += Fire;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Land.Move.performed -= OnMovemnetPerformed;
        input.Land.Move.started -= OnMovemnetStarted;
        input.Land.Move.canceled -= OnMovemnetCancelled;

        input.Land.Jump.performed -= Jump;

        input.Land.Fire.performed -= Fire;
        input.Land.Fire.canceled -= Fire;
    }


    private void FixedUpdate()
    {

        _playerVelocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        if(_moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            #region Run
            _targetSpeed = _moveDirection.normalized.magnitude * MoveSpeed;
            float speedDif = _targetSpeed - _playerVelocity;
            float accelRate = (Mathf.Abs(_targetSpeed) > 0.01f) ? Accelation : Deccelation;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

            if (isGround || (state == PlayerState.Idle || state == PlayerState.Run))
            {
                if (rb.velocity.y < 0)
                {
                    state = PlayerState.Jump;
                }
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.AddForce(moveDir.normalized * movement, ForceMode.Force);
                Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
            }
            #endregion
        }


        #region Friciton
        if (isGround && _moveDirection.magnitude < 0.01f && state != PlayerState.Jump)
        {
            Vector3 normalVelocity = rb.velocity;
            float amount = Mathf.Min(Mathf.Abs(Mathf.Abs(normalVelocity.magnitude)), Mathf.Abs(frictionAmount));
            Debug.Log(amount);
            Debug.Log("_playerVelocity : " + Mathf.Abs(normalVelocity.magnitude));
            rb.AddForce(-rb.velocity * amount, ForceMode.Impulse);

        }
        #endregion


        #region Jump Gravity
        if (rb.velocity.y < 3 && !isGround)
        {
            rb.AddForce(Physics.gravity * GravityScale, ForceMode.Acceleration);
        } 
        #endregion


        #region CheckGround
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
        transform.position.z);
        isGround = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore); 
        #endregion

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
        state = PlayerState.Idle;
        _moveDirection = Vector3.zero;


        //StartCoroutine(DecreaseMoveSpeed_co());
    }


    private void Jump(InputAction.CallbackContext obj)
    {

        //todo 1220땅에 닿았을때만 가능하게 조건 추가해줘
        if (isGround)
        {
            isGround = false;
            state = PlayerState.Jump;
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }

    }

    private void Fire(InputAction.CallbackContext obj)
    {
        if(RocketCount>0)
        {
            UseRocket();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Item"))
        {
            //아이템 획득
            Debug.Log("아이템획득");
            
            PickUpRocket();
            other.transform.gameObject.SetActive(false);
        }
    }

    public void PickUpRocket()
    {
        if(RocketCount < 3)
        {
            GameObject item = RocketPrefab;
            item.transform.localScale = new Vector3(0.5f, 1, 0.5f);
            Instantiate(item, EquidRocketPos[RocketCount].position, Quaternion.identity, RocketParent.transform);
            RocketCount++;
        }
        else
        {
            Debug.Log("더 이상 로켓을 얻을 수 없습니다.");
        }

    }

    public void UseRocket()
    {
        Instantiate(RocketPrefab, this.transform.position + Vector3.forward * 2f, transform.rotation);
        Debug.Log("로켓발싸");
        Destroy(RocketParent.transform.GetChild(3).gameObject);
        RocketCount--;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                                    transform.position.z);
        Gizmos.DrawSphere(spherePosition, GroundedRadius);
    }
}
