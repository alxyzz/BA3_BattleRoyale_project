using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Newtonsoft.Json.Linq;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _firstPersonAnimator;
    [SerializeField] private Animator _thirdPersonAnimator;
    private CharacterController _charaCtrl;

    private readonly int _fpaSpeedLevel = Animator.StringToHash("SpeedLevel");
    private readonly int _fpaMovementMultiplier = Animator.StringToHash("MovementMultiplier");

    private readonly int _tpaSpeedLevelFwd = Animator.StringToHash("SpeedLevelFwd");
    private readonly int _tpaSpeedLevelRt = Animator.StringToHash("SpeedLevelRt");
    private readonly int _tpaIsCrouch = Animator.StringToHash("IsCrouch");
    private readonly int _tpaIsInAir = Animator.StringToHash("IsInAir");

    Vector2 _lastMovementRawInput; // device input
    Vector3 _lastMovementInput; // input converted to world space

    [Header("General Settings")]
    [SerializeField] private float _gravityScale = 1.0f;

    [Header("Ground Locomotion")]
    [SerializeField] private float _maxJogSpeed = 3.0f;
    [SerializeField] private float _maxWalkSpeed = 1.9f;
    [SerializeField] private float _maxCrouchSpeed = 1.68f;
    private Vector3 _moveVelocity = Vector3.zero;
    private Vector3 _inAirVelocity = Vector3.zero;
    public float DesiredSpeed
    {
        get
        {
            if (IsCrouching) return _maxCrouchSpeed;
            if (IsWalking) return _maxWalkSpeed;
            return _maxJogSpeed;
        }
    }
    public bool IsWalking { get; set; }

    void Awake()
    {
        _charaCtrl = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        CheckOnGround();

        // Update movement
        Vector3 targetMove = _lastMovementInput * DesiredSpeed;
        if (IsOnGround)
        {
            _moveVelocity = targetMove;
        }
        else if (_lastMovementInput != Vector3.zero)
        {      
            _moveVelocity = Vector3.Lerp(_moveVelocity, targetMove, IsOnGround ? 1 : _airControl);
        }
        _charaCtrl.Move(_moveVelocity * Time.deltaTime);

        // Update jumping
        if (_charaCtrl.isGrounded && _inAirVelocity.y < 0)
        {
            _inAirVelocity.y = 0.0f;
        }
        else if (!IsOnGround)
        {
            _inAirVelocity += Physics.gravity * Time.deltaTime * _gravityScale;
        }
        _charaCtrl.Move(_inAirVelocity * Time.deltaTime);
    }

    public void AddMovementInput(Quaternion rot, Vector2 rawInput)
    {
        _lastMovementRawInput = rawInput;
        Vector3 input = new Vector3(rawInput.x, 0, rawInput.y);
        _lastMovementInput = Vector3.ClampMagnitude(rot * input, 1.0f);



        _firstPersonAnimator.SetFloat(_fpaMovementMultiplier, DesiredSpeed / _maxJogSpeed);
        _firstPersonAnimator.SetFloat(_fpaSpeedLevel, _lastMovementInput.magnitude);

        _thirdPersonAnimator.SetFloat(_fpaMovementMultiplier, DesiredSpeed / _maxJogSpeed);
        _thirdPersonAnimator.SetFloat(_tpaSpeedLevelRt, _lastMovementRawInput.x);
        _thirdPersonAnimator.SetFloat(_tpaSpeedLevelFwd, _lastMovementRawInput.y);
    }

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask _groundCheckLayers;
    public bool IsOnGround { get; private set; }
    private void CheckOnGround()
    {
        if (Physics.Raycast(
            transform.position + Vector3.up * _groundCheckDistance,
            Vector3.down,
            out RaycastHit hit,
            _groundCheckDistance * 2.0f,
            _groundCheckLayers
            ))
        {
            if (!IsOnGround)
            {
                IsOnGround = true;
                _thirdPersonAnimator.SetBool(_tpaIsInAir, false);
                OnLanded?.Invoke(hit);
            }
        }
        else IsOnGround = false;        
    }

    #region Crouch
    public bool IsCrouching { get; protected set; }
    public Action OnStartCrouching;
    public Action OnEndCrouching;
    private bool CanCrouch => !IsCrouching && IsOnGround;
    public void Crouch()
    {
        if (CanCrouch)
        {
            IsCrouching = true;
            _thirdPersonAnimator.SetBool(_tpaIsCrouch, true);
            _charaCtrl.height = 1.2f;
            _charaCtrl.center = new Vector3(0.0f, _charaCtrl.height / 2.0f, 0.0f);
            OnStartCrouching?.Invoke();
        }        
    }
    public void Uncrouch()
    {
        if (IsCrouching)
        {
            IsCrouching = false;
            _thirdPersonAnimator.SetBool(_tpaIsCrouch, false);
            _charaCtrl.height = 1.8f;
            _charaCtrl.center = new Vector3(0.0f, _charaCtrl.height / 2.0f, 0.0f);
            OnEndCrouching?.Invoke();
        }        
    }
    #endregion

    #region Jump & Fall
    [Header("Jump & Falling")]
    [SerializeField] private float _jumpUpSpeed = 4.2f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _airControl = 0.02f;
    public Action OnJumped;
    public Action<RaycastHit> OnLanded;
    private bool CanJump => IsOnGround;
    public void Jump()
    {
        if (CanJump)
        {
            // Uncrouch();
            _thirdPersonAnimator.SetBool(_tpaIsInAir, true);
            _inAirVelocity.y = _jumpUpSpeed;
            OnJumped?.Invoke();
        }
    }
    #endregion
}
