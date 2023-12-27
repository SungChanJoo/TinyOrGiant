using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Idle,
    Run,
    Jump,
    Attack,
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
    private Vector3 _inputDirection = Vector3.zero;
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
    public Camera FollowCamera;

    public GameObject RocketBullet;
    public Transform RocketPos;
    public int RocketCount=0;
    public GameObject RocketParent;
    public GameObject[] EquidRocket;

    bool _rotateOnMove = true;

    public static Action OnFired;
    private void Awake()
    {
        input = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        input.Land.Fire.performed += FirePerformed;
        input.Land.Fire.canceled += FireCanceled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Land.Move.performed -= OnMovemnetPerformed;
        input.Land.Move.started -= OnMovemnetStarted;
        input.Land.Move.canceled -= OnMovemnetCancelled;

        input.Land.Jump.performed -= Jump;

        input.Land.Fire.performed -= FirePerformed;
        input.Land.Fire.canceled -= FireCanceled;
    }

    private void Update()
    {
        //Debug.Log("_rotateOnMove" + _rotateOnMove);
        if (!_rotateOnMove)
        {
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

            Vector3 worldAimTarget = mouseWorldPosition;
            //worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = aimDirection;
        }
    }

    private void FixedUpdate()
    {


        _playerVelocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        if (_inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);
            if(_rotateOnMove) transform.rotation = Quaternion.Euler(0f, angle, 0f);

            _moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            #region Run
            _targetSpeed = _inputDirection.normalized.magnitude * MoveSpeed;
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
                rb.AddForce(_moveDirection.normalized * movement, ForceMode.Force);
                Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
            }
            #endregion
        }


        #region Friciton
        if (isGround && _inputDirection.magnitude < 0.01f && state != PlayerState.Jump)
        {
            Vector3 normalVelocity = rb.velocity;
            float amount = Mathf.Min(Mathf.Abs(Mathf.Abs(normalVelocity.magnitude)), Mathf.Abs(frictionAmount));
            //Debug.Log(amount);
            //Debug.Log("_playerVelocity : " + Mathf.Abs(normalVelocity.magnitude));
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
    }
    private void OnMovemnetPerformed(InputAction.CallbackContext value)
    {
        state = PlayerState.Run;
        _inputDirection = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);
    }
    private void OnMovemnetCancelled(InputAction.CallbackContext value)
    {
        state = PlayerState.Idle;
        _inputDirection = Vector3.zero;
    }


    private void Jump(InputAction.CallbackContext obj)
    {
        if (isGround)
        {
            isGround = false;
            state = PlayerState.Jump;
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }
    }

    private void FirePerformed(InputAction.CallbackContext obj)
    {
        //Aim UI 변경해줘 todo 1227
        Debug.Log("FirePerformed");
        if (RocketCount > 0)
        {
            SetRotateOnMove(false);
            //RocketBullet.transform.forward = _moveDirection;
            GameObject Rocket = Instantiate(RocketBullet);
            Rocket.transform.parent = RocketPos;
            Rocket.transform.position = RocketPos.transform.position;
            
            Debug.Log(Rocket.transform.localRotation);
            Rocket.transform.localRotation = Quaternion.Euler(0, 0, 90);
            Debug.Log(Rocket.transform.localRotation);
            EquidRocket[RocketCount - 1].gameObject.SetActive(false);
            RocketCount--;
        }
    }
    private void FireCanceled(InputAction.CallbackContext obj)
    {
        SetRotateOnMove(true);
        Debug.Log("FireCanceled");
        RocketPos.GetChild(0).parent = null;
        OnFired(); // 로켓 발사이벤트 로켓에서 호출
        Debug.Log("로켓발싸");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Item"))
        {
            //아이템 획득
            if(PickUpRocket())
            {
                Debug.Log("아이템획득");
                other.transform.gameObject.SetActive(false);
            }
        }
    }

    public bool PickUpRocket()
    {
        if(RocketCount < 3)
        {
            EquidRocket[RocketCount].gameObject.SetActive(true);
            RocketCount++;
            return true;
        }
        else
        {
            Debug.Log("더 이상 로켓을 얻을 수 없습니다.");
            return false;
        }
    }

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                                    transform.position.z);
        Gizmos.DrawSphere(spherePosition, GroundedRadius);
    }
}
