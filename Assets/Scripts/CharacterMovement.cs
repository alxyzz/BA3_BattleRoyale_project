using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterMovement : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _firstPersonAnimator;
    [SerializeField] private Animator _thirdPersonAnimator;
    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;

    private readonly int _fpaSpeedLevel = Animator.StringToHash("SpeedLevel");
    private readonly int _fpaMovementMultiplier = Animator.StringToHash("MovementMultiplier");

    private readonly int _tpaSpeedLevelFwd = Animator.StringToHash("SpeedLevelFwd");
    private readonly int _tpaSpeedLevelRt = Animator.StringToHash("SpeedLevelRt");

    Vector2 _lastMovementRawInput; // device input
    Vector3 _lastMovementInput; // input converted to world space

    [Header("Ground Locomotion")]
    [SerializeField] private float _maxJogSpeed = 3.0f;
    [SerializeField] private float _maxWalkSpeed = 1.9f;
    [SerializeField] private float _maxCrouchSpeed = 1.68f;
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
    private bool _isCrouching;
    public bool IsCrouching
    {
        get { return _isCrouching; }
        set
        {
            _isCrouching = value;
            _collider.height = _isCrouching ? 1.0f : 1.8f;
            _collider.center = new Vector3(0.0f, _collider.height / 2.0f, 0.0f);
        }
    }

    [Header("Jump & Falling")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _airControl = 0.15f;
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        if (!isLocalPlayer) Destroy(this);
    }

    private void FixedUpdate()
    {
        CheckOnGround();
        FixedUpdateMovement();
    }

    public void AddMovementInput(Quaternion rot, Vector2 rawInput)
    {
        _lastMovementRawInput = rawInput;
        Vector3 input = new Vector3(rawInput.x, 0, rawInput.y);
        _lastMovementInput = Vector3.ClampMagnitude(rot * input, 1.0f);
    }
    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.02f;
    [SerializeField] private LayerMask _groundCheckLayers;
    public bool IsOnGround { get; private set; }

    private void CheckOnGround()
    {
        IsOnGround = Physics.CheckSphere(transform.position, _groundCheckRadius, _groundCheckLayers);
        // Debug.Log(IsOnGround);
    }
    
    private void FixedUpdateMovement()
    {
        if (_lastMovementInput != Vector3.zero)
        {
            Vector3 targetVelocity = _lastMovementInput * DesiredSpeed;

            targetVelocity += Vector3.up * _rigidbody.velocity.y;

            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, IsOnGround ? 1.0f : _airControl);

            //_firstPersonAnimator.SetFloat(_fpaSpeedLevel, IsCrouching || IsWalking ? 0.5f : 1.0f);
        }

        _firstPersonAnimator.SetFloat(_fpaMovementMultiplier, DesiredSpeed / _maxJogSpeed);
        _firstPersonAnimator.SetFloat(_fpaSpeedLevel, _lastMovementInput.magnitude);

        _thirdPersonAnimator.SetFloat(_fpaMovementMultiplier, DesiredSpeed / _maxJogSpeed);
        _thirdPersonAnimator.SetFloat(_tpaSpeedLevelRt, _lastMovementRawInput.x);
        _thirdPersonAnimator.SetFloat(_tpaSpeedLevelFwd, _lastMovementRawInput.y);

    }
}
