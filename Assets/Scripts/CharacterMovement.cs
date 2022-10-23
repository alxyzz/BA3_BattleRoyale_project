using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Newtonsoft.Json.Bson;

public class CharacterMovement : NetworkBehaviour
{
    [Header("Components")]
    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    [SerializeField] private Animator _firstPersonAnimator;
    [SerializeField] private Animator _thirdPersonAnimator;

    private readonly int _fpaSpeedLevel = Animator.StringToHash("SpeedLevel");

    Vector3 _lastMovementInput;

    [Header("Ground Locomotion")]
    [SerializeField] private float _maxJogSpeed = 2.7f;
    [SerializeField] private float _maxWalkSpeed = 1.8f;
    [SerializeField] private float _maxCrouchSpeed = 1.2f;
    public bool IsWalking { get; set; }
    private bool _isCrouching;
    public bool IsCrouching
    {
        get { return _isCrouching; }
        set
        {
            _isCrouching = value;
            _collider.height = _isCrouching ? 1.0f : 1.8f;
            _collider.center.Set(0.0f, _collider.height / 2.0f, 0.0f);
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

    private void FixedUpdate()
    {
        CheckOnGround();
        FixedUpdateMovement();
    }

    public void AddMovementInput(Vector3 input)
    {
        _lastMovementInput = Vector3.ClampMagnitude(input, 1.0f);
    }
    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.02f;
    [SerializeField] private LayerMask _groundCheckLayers;
    public bool IsOnGround { get; private set; }

    private void CheckOnGround()
    {
        IsOnGround = Physics.CheckSphere(transform.position, _groundCheckRadius, _groundCheckLayers);
        Debug.Log(IsOnGround);
    }
    
    private void FixedUpdateMovement()
    {
        if (_lastMovementInput != Vector3.zero)
        {
            Debug.Log(_lastMovementInput);
            Vector3 targetVelocity = _lastMovementInput * _maxJogSpeed + Vector3.up * _rigidbody.velocity.y;

            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, IsOnGround ? 1.0f : _airControl);
        }
    }
}
