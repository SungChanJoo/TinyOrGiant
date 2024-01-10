using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

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
    [SyncVar(hook = nameof(GroundChange))]
    public bool isGround;
    void GroundChange(bool _old, bool _new) { }
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Grappling")]
    public float GrapplingSpeed = 10f;
    public float Offset_y = 0.5f;
    public float MaxGrappleDistance;

    private bool grappling;
    private Vector3 grapplePoint;
    private Vector3 grappleMoveVector;

    public float GrapplingCd;
    private float grapplingCdTimer;

    private float _targetAngleY;
    public Transform GunTip;
    [SyncVar(hook = nameof(ChangeGunTipPos))]
    public Vector3 GunTipPos;
    void ChangeGunTipPos(Vector3 _old, Vector3 _new) { }
    public LayerMask WhatIsGrappleable;
    public LineRenderer Lr;

    [Header("Dash")]
    public float DashSpeed;
    public float DashDistance;
    public float DashCd;
    private float dashCdTimer;
    public ParticleSystem DashEffect;

    [Header("Rocket")]
    public GameObject RocketBullet;
    public Transform RocketPos;
    public int RocketCount = 0;
    public GameObject RocketParent;
    public GameObject[] EquidRocket;
    public float RocketPower = 3f;
    public bool IsFireReady = false;
    public ParticleSystem RocketJumpEffect;

    [Header("Ragdoll")]
    public float ragdollOffset = 0.9f;
    public Transform RigPelvis;
    public bool isRagdoll = false;
    public List<Collider> ragdollColliders = new List<Collider>();
    public List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("ETC")]
    bool _rotateOnMove = true;
    public bool Freeze = false;
    public bool ActiveGrapple;
    public bool enableMovementOnNextTouch;
    public Transform AimTarget;
    public RigBuilder PlayerRig;
    public float AimDistance =1f;
    public bool IsGrab;
    public Collider PlayerColl;

    private Animator _animator;
    private Ray ray;

    private int grapplingCount = 0;

    public static Action<Vector3> OnFired;
    private void Awake()
    {
        input = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        //Cursor.visible = false;
        state = PlayerState.Idle;
        isGround = true;
        IsGrab = false;
        PlayerRig.enabled = false;
        if (GameManager.Instance.playerType != PlayerType.PC) return;
        _freeLook = GameObject.FindGameObjectWithTag("PCPlayerCam").GetComponent<CinemachineFreeLook>();
        Cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        Cursor.lockState = CursorLockMode.Locked;

        // Ragdoll Collider 탐색
        var colliders = FindObjectsOfType<Collider>();
        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                ragdollColliders.Add(collider);
        }

        // Ragdoll Rigidbody 탐색
        var rigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (var rigidbody in rigidbodies)
        {
            if (rigidbody.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                ragdollRigidbodies.Add(rigidbody);
        }
    }


    [Command(requiresAuthority = false)]
    private void CmdToggleRagdoll(bool isTurnOn)
    {
        RpcToggleRagdoll(isTurnOn);
    }
    [ClientRpc]
    private void RpcToggleRagdoll(bool isTurnOn)
    {
        isRagdoll = isTurnOn;
        if(!isRagdoll)
        {
            StartCoroutine(DelayRagdoll(isRagdoll));
        }
        else
        {
            ToggleRagdoll(isTurnOn);
            _animator.enabled = !isRagdoll;
        }
    }
    IEnumerator DelayRagdoll(bool isTurnOn)
    {
        yield return new WaitForSeconds(1f);
        ToggleRagdoll(isTurnOn);
        _animator.enabled = !isRagdoll;
    }

    private void ToggleRagdoll(bool isTurnOn)
    {
        foreach (var ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = isTurnOn;
        }
        foreach (var ragdollRigidbody in ragdollRigidbodies)
        {
            ragdollRigidbody.isKinematic = !isTurnOn;
            ragdollRigidbody.useGravity = isTurnOn;
        }
    }

    private void OnEnable()
    {
        input.Enable();
        input.Land.Move.performed += OnMovemnetPerformed;
        input.Land.Move.started += OnMovemnetStarted;
        input.Land.Move.canceled += OnMovemnetCanceled;

        input.Land.Jump.performed += Jump;

        input.Land.Fire.performed += FirePerformed;
        input.Land.Fire.canceled += FireCanceled;

        input.Land.Grappling.performed += GrapplingPerformed;
        input.Land.Grappling.canceled += GrpplingCanceled;

        input.Land.Dash.performed += DashPerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Land.Move.performed -= OnMovemnetPerformed;
        input.Land.Move.started -= OnMovemnetStarted;
        input.Land.Move.canceled -= OnMovemnetCanceled;

        input.Land.Jump.performed -= Jump;

        input.Land.Fire.performed -= FirePerformed;
        input.Land.Fire.canceled -= FireCanceled;

        input.Land.Grappling.performed -= GrapplingPerformed;
        input.Land.Grappling.canceled -= GrpplingCanceled;

        input.Land.Dash.performed -= DashPerformed;
    }

    public override void OnStartLocalPlayer()
    {
        _freeLook.Follow = this.transform;
        _freeLook.LookAt = this.transform;
    }


    [Command]
    private void CmdPlayTriggerAni(string aniName)
    {
        RpcPlayTriggerAni(aniName);
    }
    [ClientRpc]
    private void RpcPlayTriggerAni(string aniName)
    {
        _animator.SetTrigger(aniName);
        Debug.Log(aniName);
    }
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        //if(isRagdoll)
        //{
        //    // Hip을 기준으로 Transform 동기화
        //    transform.position = RigPelvis.position;
        //    return;
        //}
        if (IsGrab) return;
        
        if (rb.velocity.magnitude < 0.01f)
        {
            _animator.SetFloat("x", rb.velocity.x);
            _animator.SetFloat("y", rb.velocity.z);
        }
        #region CheckGround
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
        transform.position.z);
        if (!Freeze)
            isGround = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
        if (_animator != null)
            _animator.SetBool("isGround", isGround);
        if (isGround)
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0f);


        #endregion
        #region Grappling
        if (state == PlayerState.Grappling)
        {
            Vector3 dir = grapplePoint - transform.position;
            Debug.Log(dir.magnitude * Offset_y);
            rb.velocity = (grappleMoveVector + new Vector3(0, dir.magnitude * Offset_y, 0)) * GrapplingSpeed;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
            transform.rotation = Quaternion.Euler(30f, transform.eulerAngles.y, 0f);
            
        }
        #endregion

        #region Run
        _playerVelocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);

        if (_inputDirection.magnitude >= 0.1f && !Freeze)
        {
            #region CameraMovement
            float targetAngle = Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);
            //평상시 회전
            if (_rotateOnMove && !Freeze)
            {
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
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
                if (_animator != null && !IsFireReady)
                {
                    _animator.SetFloat("x", _moveDirection.normalized.x);
                    _animator.SetFloat("y", _moveDirection.normalized.z);
                }
                rb.AddForce(_moveDirection.normalized * movement, ForceMode.Force);
                //Debug.Log(_moveDirection.normalized);
                //Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
            }
            #endregion
        }
        #endregion

        #region Friciton
        if (isGround && _inputDirection.magnitude < 0.01f && state != PlayerState.Jump && state != PlayerState.Grappling && !Freeze)
        {
            Vector3 normalVelocity = rb.velocity;
            float amount = Mathf.Min(Mathf.Abs(Mathf.Abs(normalVelocity.magnitude)), Mathf.Abs(frictionAmount));
            if(_animator != null)
            {
                _animator.SetFloat("x", rb.velocity.x);
                _animator.SetFloat("y", rb.velocity.z);
            }

            rb.AddForce(-rb.velocity * amount, ForceMode.Impulse);
           // Debug.Log("Friciton : " +(-rb.velocity * amount));
        }
        #endregion

        #region Jump Gravity
        if (rb.velocity.y < 3.5f && !isGround)
        {
            rb.AddForce(Physics.gravity * GravityScale, ForceMode.Acceleration);
        }
        #endregion

    }

    
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (isRagdoll)
        {
            RigPelvis.position = transform.position - Vector3.up * ragdollOffset;
            return;
        }
        if (IsGrab) return;
        if (GameManager.Instance.playerType != PlayerType.PC) return;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        #region Grappling
        if (Vector3.SqrMagnitude(transform.position - grapplePoint) < 10f && grappling)
        {
            CmdStopGrappling();
        }
        if (grapplingCdTimer > 0)
        {
            grapplingCdTimer -= Time.deltaTime;
            //Debug.Log("grapplingCdTimer :" + grapplingCdTimer);
        }

        #endregion

        if (dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;
            //Debug.Log("dashCdTimer :" +dashCdTimer);
        }

        //조준시 카메라,플레이어 회전
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
            CmdAimTargetPos(Cam.position + Cam.forward * AimDistance);
            transform.forward = new Vector3(aimDirection.x, 0, aimDirection.z);
            if (_animator != null && IsFireReady && _inputDirection.magnitude >= 0.1f && !Freeze)
            {
                Debug.Log("transform.forward" + transform.forward);
                _animator.SetFloat("x", _inputDirection.x);
                _animator.SetFloat("y", _inputDirection.z);
            }
        }
        #endregion
    }
    [Command]
    private void CmdAimTargetPos(Vector3 pos)
    {
        RpcAimTargetPos(pos);
    }
    [ClientRpc]
    private void RpcAimTargetPos(Vector3 pos)
    {
        AimTarget.position = pos;

    }
    private void LateUpdate()
    {
        if (!isLocalPlayer) return;

        if (grappling)
        {
            CmdSetGrappling(GunTip.position);
        }
    }

    #region Player Input System

    #region WASD : Move
    private void OnMovemnetStarted(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer) return;
        if (IsGrab) return;

        if (!Freeze)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
    private void OnMovemnetPerformed(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer || state == PlayerState.Grappling) return;
        if (IsGrab) return;

        state = PlayerState.Run;
        _inputDirection = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);


    }
    private void OnMovemnetCanceled(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer ) return;
        if (IsGrab) return;
        if(state != PlayerState.Grappling)
            state = PlayerState.Idle;
        _inputDirection = Vector3.zero;
    }
    #endregion

    #region SpaceBar : Jump
    private void Jump(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;
        if (Freeze) return;
        if (IsGrab)
        {
            if (RocketCount > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * JumpPower * RocketPower, ForceMode.Impulse);
                CmdPlayRocketJumpEffect();
                if (IsGrab)
                    IsGrab = false;
            }
            else
                return;
        }
        if (isGround)
        {
            isGround = false;
            if (_animator != null)
                CmdPlayTriggerAni("Jump");
                //_animator.SetTrigger("Jump");
            state = PlayerState.Jump;
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }
        else if (RocketCount > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * JumpPower * RocketPower, ForceMode.Impulse);
            CmdPlayRocketJumpEffect();
        }
    }
    [Command]
    private void CmdPlayRocketJumpEffect()
    {
        RpcPlayRocketJumpEffect();
    }
    [ClientRpc]
    private void RpcPlayRocketJumpEffect()
    {
        StartCoroutine(PlayRocketJumpEffect_co());
    }

    private IEnumerator PlayRocketJumpEffect_co()
    {
        EquidRocket[RocketCount - 1].gameObject.SetActive(false);
        RocketCount--;
        RocketJumpEffect.Play();
        yield return new WaitForSeconds(2f);
        RocketJumpEffect.Stop();
    }

    #endregion

    #region MouseLeftButton : FireRocket
    private void FirePerformed(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;
        if (IsGrab) return;
        if (Freeze) return;
        //Aim UI 변경해줘 todo 1227
        if (RocketCount > 0)
        {
            CmdFireReadyRocket();
        }
    }
    private void FireCanceled(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;
        if (IsGrab) return;
        if (Freeze) return;

        CmdFireRocket(ray);
    }
    #endregion

    #region MouseRightButton : Grappling 
    private void GrapplingPerformed(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;
        if (IsGrab) return;
        if (IsFireReady) return;

        if (grapplingCdTimer > 0 || Freeze) return;

        if (Physics.Raycast(ray, out RaycastHit hit, 999f))
        {
            _targetAngleY = transform.rotation.y;
            rb.velocity = new Vector3(0, 0, 0);
            enableMovementOnNextTouch = true;

            grapplePoint = hit.point;
            GunTipPos = grapplePoint;
            CmdSetGrappling(GunTipPos);
            grappleMoveVector = ray.direction;


            CmdViewGrappling(grapplePoint);
            //_animator.SetTrigger("Grappling");
            CmdPlayTriggerAni("Grappling");

            _animator.SetBool("isGrappling", true);
            state = PlayerState.Grappling;
            Freeze = true;
        }
        else
        {
            Debug.Log("그곳은 그래플링할 수 없습니다.");
        }

    }

    private void GrpplingCanceled(InputAction.CallbackContext obj)
    {
        if (!isLocalPlayer) return;
        if (IsGrab) return;
        if (IsFireReady) return;

        if (Freeze)
            CmdStopGrappling();
    }
    #endregion

/*    #region KeyboardButton : Ragdoll
    private void OnRagdollPerformed(InputAction.CallbackContext context)
    {
        isRagdoll = !isRagdoll;
        ToggleRagdoll(isRagdoll);

        IsGrab = isRagdoll;


    }*/



    private void DashPerformed(InputAction.CallbackContext obj)
    {
        //Debug.Log(_inputDirection.magnitude);
        if (_inputDirection.magnitude < 0.01f || !isGround || dashCdTimer > 0 || Freeze) return;
        if (IsGrab) return;
        if (IsFireReady) return;

        dashCdTimer = DashCd;
        float targetAngle = Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        if (_animator != null)
            CmdPlayTriggerAni("Dash");

        //_animator.SetTrigger("Dash");
        rb.velocity = _moveDirection.normalized * DashSpeed;
        Freeze = true;
        CmdPlayDashEffect();
        //StartCoroutine(DashFreeze_co());
    }

    #endregion
    [Command]
    private void CmdPlayDashEffect()
    {
        RpcPlayDashEffect();
    }
    [ClientRpc]
    private void RpcPlayDashEffect()
    {
        StartCoroutine(DashFreeze_co());
    }
    IEnumerator DashFreeze_co()
    {
        DashEffect.Play();
        while (true)
        {
            yield return new WaitForSeconds(DashDistance * 0.8f);
            DashEffect.Stop();
            yield return new WaitForSeconds(DashDistance * 0.1f);
            break;
        }
        Freeze = false;
        if (state == PlayerState.Idle)
            _inputDirection = Vector3.zero;
    }
    private void StopGrappling()
    {

        Freeze = false;
        grappling = false;
        grapplingCdTimer = GrapplingCd;
        Lr.enabled = false;
        //state = PlayerState.Idle;
        _animator.SetBool("isGrappling", false);
        state = PlayerState.Jump;

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

        Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(transform.position, 4f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer) return;

        if (enableMovementOnNextTouch)
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                                                transform.position.z);
            isGround = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                                    QueryTriggerInteraction.Ignore);
            if (!isGround)
            {
                CmdStopGrappling();
                rb.AddForce(-(grappleMoveVector + Vector3.down) * 10f, ForceMode.Impulse);
                enableMovementOnNextTouch = false;
            }
            else
            {
                CmdStopGrappling();
                enableMovementOnNextTouch = false;
            }
        }
    }

    // Grab Interactable Event
    public void Grabbed(bool isGrabbed)
    {
        CmdUpdateToHandPosition(isGrabbed);
        CmdToggleRagdoll(isGrabbed);
    }

    private IEnumerator currentUpdatePos = null;
    private IEnumerator UpdateToHandTransform(GameObject targetObj)
    {
        while (true)
        {
            transform.position = targetObj.transform.position;
            transform.rotation = targetObj.transform.rotation;
            yield return null;
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdUpdateToHandPosition(bool isGrabbed)
    {
        RpcUpdateToHandPosition(isGrabbed);
    }
    [ClientRpc]
    private void RpcUpdateToHandPosition(bool isGrabbed)
    {
        IsGrab = isGrabbed;
        Collider[] handColliders = Physics.OverlapSphere(transform.position, 4f);

        // 잡았으면 손의 자식
        if (IsGrab)
        {
            foreach (var handCollider in handColliders)
            {
                if (handCollider.gameObject.layer == LayerMask.NameToLayer("Left Hand Physics"))
                {
                    //CmdChangeParent(handCollider.gameObject);

                    if (currentUpdatePos != null)
                    {
                        StopCoroutine(currentUpdatePos);
                        currentUpdatePos = null;
                    }
                    currentUpdatePos = UpdateToHandTransform(handCollider.gameObject);
                    StartCoroutine(currentUpdatePos);

                    Debug.Log("왼손 잡기!");
                    return;
                }
                else if (handCollider.gameObject.layer == LayerMask.NameToLayer("Right Hand Physics"))
                {
                    //CmdChangeParent(handCollider.gameObject);

                    if (currentUpdatePos != null)
                    {
                        StopCoroutine(currentUpdatePos);
                        currentUpdatePos = null;
                    }
                    currentUpdatePos = UpdateToHandTransform(handCollider.gameObject);
                    StartCoroutine(currentUpdatePos);

                    Debug.Log("오른손 잡기!");
                    return;
                }
            }
        }
        // 놓았으면 원래 대로
        else
        {
            foreach (var handCollider in handColliders)
            {
                if (handCollider.gameObject.layer == LayerMask.NameToLayer("Left Hand Physics"))
                {
                    //CmdChangeParent(null);

                    if (currentUpdatePos != null)
                    {
                        StopCoroutine(currentUpdatePos);
                        currentUpdatePos = null;
                    }

                    Debug.Log("왼손 놓기!");
                    return;
                }
                else if (handCollider.gameObject.layer == LayerMask.NameToLayer("Right Hand Physics"))
                {
                    //CmdChangeParent(null);

                    if (currentUpdatePos != null)
                    {
                        StopCoroutine(currentUpdatePos);
                        currentUpdatePos = null;
                    }

                    Debug.Log("오른손 놓기!");
                    return;
                }
            }
        }
    }

    /*    IEnumerator CollisionFreeze_co()
        {

            Freeze = true;
            yield return new WaitForSeconds(0.3f);
            Freeze = false;
            if (state == PlayerState.Idle)
                _inputDirection = Vector3.zero;
        }*/
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("Item"))
        {
            //아이템 획득
            CmdPickUpRocket(other.transform.gameObject);
        }
    }

    #region Command
    #region Grappling

    [Command]
    public void CmdViewGrappling(Vector3 grapplePoint)
    {

        RpcViewGrappling(grapplePoint);
    }

    [Command]
    public void CmdSetGrappling(Vector3 GunTip)
    {
        RpcSetGrappling(GunTip);
    }

    [Command]
    public void CmdStopGrappling()
    {
        RpcStopGrappling();
    }
    #endregion

    #region FireRocket
    [Command]
    public void CmdFireReadyRocket()
    {
        GameObject Rocket = Instantiate(RocketBullet, new Vector3(0, 0, 0), Quaternion.identity);
        NetworkServer.Spawn(Rocket);

        RPCFireReadyRocket(Rocket);
    }

    [Command]
    public void CmdFireRocket(Ray ray)
    {
        RPCFireRocket(ray);
    }
    #endregion

    [Command(requiresAuthority = false)]
    public void CmdPickUpRocket(GameObject rocket)
    {
        RpcPickUpRocket(rocket);
    }

    #endregion

    #region ClientRpc
    #region Grappling

    [ClientRpc]
    public void RpcViewGrappling(Vector3 grapplePoint)
    {
        Lr.SetPosition(1, grapplePoint);
        grappling = true;
        Lr.enabled = true;
    }


    [ClientRpc]
    public void RpcSetGrappling(Vector3 gunTip)
    {
        Lr.SetPosition(0, gunTip);
    }

    [ClientRpc]
    public void RpcStopGrappling()
    {
        StopGrappling();
    }
    #endregion
    #region FireRocket

    [ClientRpc]
    public void RPCFireReadyRocket(GameObject Rocket)
    {
        PlayerRig.enabled = true;

        IsFireReady = true;
        _animator.SetBool("isAiming", IsFireReady);
        SetRotateOnMove(false);
        Rocket.transform.parent = RocketPos;
        Rocket.transform.position = RocketPos.transform.position;
        Rocket.transform.localRotation = Quaternion.Euler(0, -90, 0);
        EquidRocket[RocketCount - 1].gameObject.SetActive(false);
        RocketCount--;
    }
    [ClientRpc]
    public void RPCFireRocket(Ray ray)
    {
        if (IsFireReady)
        {

            SetRotateOnMove(true);
            RocketPos.GetChild(0).parent = null;
            Vector3 targetVector = new Vector3(ray.direction.x, ray.direction.y, ray.direction.z);

            OnFired(targetVector); // 로켓 발사이벤트 로켓에서 호출
            IsFireReady = false;
            _animator.SetBool("isAiming", IsFireReady);
            PlayerRig.enabled = false;
        }

    }
    #endregion
    [ClientRpc]
    public void RpcPickUpRocket(GameObject rocket)
    {
        if (RocketCount < 3)
        {
            EquidRocket[RocketCount].gameObject.SetActive(true);
            RocketCount++;
            if (rocket != null)
            {
                rocket.SetActive(false);
            }
        }
    }
    #endregion
}
