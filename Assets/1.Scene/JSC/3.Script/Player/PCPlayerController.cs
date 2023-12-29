using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;

public enum PlayerState
{
    Idle,
    Run,
    Jump,
    Attack,
    Grappling,
    Freeze
}


public class PCPlayerController : NetworkBehaviour
{
    public Transform cam;

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
    [SerializeField] CinemachineFreeLook _freeLook;

    [Header("Player Grounded")]
    public float GravityScale = 5f;
    public bool isGround;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Grappling")]
    public float GrapplingSpeed = 10f;
    public float Offset_y = 0.5f;
    public float MaxGrappleDistance;
    public float GrappleDelayTime;

    private bool grappling;
    private Vector3 grapplePoint;
    private Vector3 grappleMoveVector;

    public float GrapplingCd;
    private float grapplingCdTimer;

    public Transform GunTip;
    public LayerMask WhatIsGrappleable;
    public LineRenderer Lr;



    [Header("Rocket")]
    public GameObject RocketBullet;
    public Transform RocketPos;
    public int RocketCount=0;
    public GameObject RocketParent;
    public GameObject[] EquidRocket;
    public float RocketPower = 3f;
    public bool IsFireReady = false;

    [Header("ETC")]
    public Camera FollowCamera;
    bool _rotateOnMove = true;
    public bool Freeze = false;
    public bool ActiveGrapple;

    private Ray ray;

    public static Action OnFired;
    private void Awake()
    {
        input = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        Cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _freeLook = GameObject.FindGameObjectWithTag("PCPlayerCam").GetComponent<CinemachineFreeLook>();


        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
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

        input.Land.Grappling.performed += GrpplingPerformed;
        input.Land.Grappling.canceled += GrpplingCanceled;
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

        input.Land.Grappling.performed -= GrpplingPerformed;
        input.Land.Grappling.canceled -= GrpplingCanceled;
    }

    public override void OnStartLocalPlayer()
    {
        _freeLook.Follow = this.transform;
        _freeLook.LookAt = this.transform;
    }

    private void FixedUpdate()
    {

        if (!isLocalPlayer) return;

        #region Grappling
        if (state == PlayerState.Grappling)
        {
            Debug.Log("ray.direction :" + ray.direction);
            rb.velocity = (grappleMoveVector + new Vector3(0, Offset_y, 0))*GrapplingSpeed;
            //rb.AddForce((ray.direction+ new Vector3(0,Offset_y,0)) * GrapplingSpeed, ForceMode.Impulse);
        }
        #endregion

        #region Run
        _playerVelocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        if (_inputDirection.magnitude >= 0.1f && !Freeze)
        {
            #region CameraMovement
            float targetAngle = Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);
            if (_rotateOnMove) transform.rotation = Quaternion.Euler(0f, angle, 0f);

            _moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; 
            #endregion

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
                Debug.Log(_moveDirection.normalized);
                //Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
            }
            #endregion
        } 
        #endregion

        #region Friciton
        if (isGround && _inputDirection.magnitude < 0.01f && state != PlayerState.Jump && state != PlayerState.Grappling)
        {
            Vector3 normalVelocity = rb.velocity;
            float amount = Mathf.Min(Mathf.Abs(Mathf.Abs(normalVelocity.magnitude)), Mathf.Abs(frictionAmount));
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
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (GameManager.Instance.playerType != PlayerType.PC) return;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Vector3.SqrMagnitude(transform.position- grapplePoint) <10f)
        {
            StopGrappling();
        }
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;


        #region CameraMovement
        if (!_rotateOnMove)
        {
            Vector3 mouseWorldPosition = Vector3.zero;
            if (Physics.Raycast(ray, out RaycastHit hit, 999f))
            {
                mouseWorldPosition = hit.point;
            }
            Debug.DrawRay(transform.position, ray.direction * 999f, Color.red);
            Vector3 aimDirection = (mouseWorldPosition - transform.position).normalized;
            transform.forward = aimDirection;
        } 
        #endregion
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;

        if (grappling)
        {
            Lr.SetPosition(0, GunTip.position);
        }
    }
    private void OnMovemnetStarted(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer) return;

        if (!Freeze)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
    private void OnMovemnetPerformed(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer) return;

        if (!Freeze)
        {
            state = PlayerState.Run;
            _inputDirection = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);
        }

    }
    private void OnMovemnetCancelled(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer) return;

        state = PlayerState.Idle;
        _inputDirection = Vector3.zero;
    }


    private void Jump(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;

        if (isGround)
        {
            isGround = false;
            state = PlayerState.Jump;
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }
        else if( RocketCount >0)
        {
            rb.AddForce(Vector3.up * JumpPower * RocketPower, ForceMode.Impulse);
            EquidRocket[RocketCount - 1].gameObject.SetActive(false);
            RocketCount--;
        }
    }

    private void FirePerformed(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;

        //Aim UI 변경해줘 todo 1227

        CmdFireReadyRocket();
    }
    [Command]
    public void CmdFireReadyRocket()
    {
        RPCFireReadyRocket();
    }
    [ClientRpc]
    public void RPCFireReadyRocket()
    {
        if (RocketCount > 0)
        {
            IsFireReady = true;
            SetRotateOnMove(false);
            GameObject Rocket = Instantiate(RocketBullet);
            Rocket.transform.parent = RocketPos;
            Rocket.transform.position = RocketPos.transform.position;

            Rocket.transform.localRotation = Quaternion.Euler(0, 0, 90);
            EquidRocket[RocketCount - 1].gameObject.SetActive(false);
            RocketCount--;
        }
    }


    private void FireCanceled(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;

        CmdFireRocket();
    }

    [Command]
    public void CmdFireRocket()
    {
        RPCFireRocket();
    }
    [ClientRpc]
    public void RPCFireRocket()
    {
        if(IsFireReady)
        {
            SetRotateOnMove(true);
            RocketPos.GetChild(0).parent = null;
            OnFired(); // 로켓 발사이벤트 로켓에서 호출
            IsFireReady = false;
        }

    }
    private void GrpplingPerformed(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;

        if (Physics.Raycast(ray, out RaycastHit hit, 999f))
        {
            transform.LookAt(hit.point);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            enableMovementOnNextTouch = true;
            grappling = true;
            grapplePoint = hit.point;
            grappleMoveVector = ray.direction;

            Lr.enabled = true;
            Lr.SetPosition(1, grapplePoint);
            state = PlayerState.Grappling;
            Freeze = true;
        }

    }
    private void GrpplingCanceled(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;

        StopGrappling();
    }
    private bool enableMovementOnNextTouch;

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer) return;

        Debug.Log("OnCollisionEnter : " + collision.transform.name);
        if (enableMovementOnNextTouch)
        {
            rb.AddForce(-(grappleMoveVector+Vector3.down)*10f, ForceMode.Impulse);
            StopGrappling();    
            enableMovementOnNextTouch = false;
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("Item"))
        {
            //아이템 획득
            if(PickUpRocket())
            {
                Debug.Log("아이템획득");
                other.transform.gameObject.SetActive(false);
            }
        }
    }

    private void StopGrappling()
    {

        Freeze = false;
        grappling = false;
        grapplingCdTimer = GrapplingCd;
        Lr.enabled = false;
        state = PlayerState.Idle;
    }

    public bool PickUpRocket()
    {

        if (RocketCount < 3)
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
