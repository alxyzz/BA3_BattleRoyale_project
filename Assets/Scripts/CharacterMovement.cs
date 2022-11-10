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
    // [Header("Components")]
    // [SerializeField] private Animator _firstPersonAnimator;
    // [SerializeField] private Animator _thirdPersonAnimator;
    private CharacterAnimHandler _charaAnimHandler;
    private CharacterController _charaCtrl;
    private PlayerState _playerState;

    private readonly int _aSpeedLevel = Animator.StringToHash("SpeedLevel");
    private readonly int _aMovementMultiplier = Animator.StringToHash("MovementMultiplier");

    private readonly int _aSpeedLevelFwd = Animator.StringToHash("SpeedLevelFwd");
    private readonly int _aSpeedLevelRt = Animator.StringToHash("SpeedLevelRt");
    private readonly int _aIsCrouch = Animator.StringToHash("IsCrouch");
    private readonly int _aIsInAir = Animator.StringToHash("IsInAir");

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
            float weapon = 1.0f;
            if (_playerState.CurrentWeaponIdentity != null) weapon = _playerState.CurrentWeaponIdentity.Data.MovementMultiplier;
            if (IsCrouching) return _maxCrouchSpeed * weapon;
            if (IsWalking) return _maxWalkSpeed * weapon;
            return _maxJogSpeed * weapon;
        }
    }
    public float RecoilMultiplier
    {
        get
        {
            if (!IsOnGround) return 4.0f;
            if (IsCrouching) return 0.6f;
            if (IsWalking) return 0.8f;
            if (_lastMovementInput != Vector3.zero) return _lastMovementInput.sqrMagnitude + 1.0f;
            return 1.0f;
        }
    }
    public float SpreadMultiplier
    {
        get
        {
            if (!IsOnGround) return 10.0f;
            if (IsCrouching) return 0.6f;
            if (IsWalking) return 0.8f;
            if (_lastMovementInput != Vector3.zero) return _lastMovementInput.sqrMagnitude * 4.0f + 1.0f;
            return 1.0f;
        }
    }
    public bool IsWalking { get; set; }

    void Awake()
    {
        _charaAnimHandler = GetComponent<CharacterAnimHandler>();
        _charaCtrl = GetComponent<CharacterController>();
        _playerState = GetComponent<PlayerState>();
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

        // Update Crosshair
        UpdateCrosshairSpread();
    }

    public void AddMovementInput(Quaternion rot, Vector2 rawInput)
    {
        _lastMovementRawInput = rawInput;
        Vector3 input = new Vector3(rawInput.x, 0, rawInput.y);
        _lastMovementInput = Vector3.ClampMagnitude(rot * input, 1.0f);

        _charaAnimHandler.FpSetFloat(_aMovementMultiplier, DesiredSpeed / _maxJogSpeed);
        _charaAnimHandler.FpSetFloat(_aSpeedLevel, _lastMovementInput.magnitude);

        _charaAnimHandler.CmdTpSetFloat(_aMovementMultiplier, DesiredSpeed / _maxJogSpeed);
        _charaAnimHandler.CmdTpSetFloat(_aSpeedLevelRt, _lastMovementRawInput.x);
        _charaAnimHandler.CmdTpSetFloat(_aSpeedLevelFwd, _lastMovementRawInput.y);
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
                _charaAnimHandler.CmdTpSetBool(_aIsInAir, false);
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
            _charaAnimHandler.CmdTpSetBool(_aIsCrouch, true);
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
            _charaAnimHandler.CmdTpSetBool(_aIsCrouch, false);
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
            _charaAnimHandler.CmdTpSetBool(_aIsInAir, true);
            _inAirVelocity.y = _jumpUpSpeed;
            OnJumped?.Invoke();
        }
    }
    #endregion

    private void UpdateCrosshairSpread()
    {
        if (IsOnGround)
        {
            if (IsCrouching)
                UIManager.SetCrosshairMovementSpread(0);
            else if (_lastMovementInput != Vector3.zero)
            {
                if (IsWalking)
                    UIManager.SetCrosshairMovementSpread(30 * _lastMovementInput.sqrMagnitude);
                else
                    UIManager.SetCrosshairMovementSpread(100 * _lastMovementInput.sqrMagnitude);
            }
            else
                UIManager.SetCrosshairMovementSpread(5);
        }
        else
            UIManager.SetCrosshairMovementSpread(200);
    }
}
