using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterMovement : NetworkBehaviour
{
    Rigidbody _rigidbody;


    [Header("Movement")]
    [SerializeField] float _speed = 5.0f;
    Vector3 _lastMovementInput;
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_lastMovementInput != Vector3.zero)
        {
            _rigidbody.MovePosition(_rigidbody.position + _lastMovementInput * _speed * Time.fixedDeltaTime);
        }
    }

    public void AddMovement(Vector3 input)
    {
        _lastMovementInput = input;
    }
}
