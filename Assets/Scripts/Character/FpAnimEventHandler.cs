using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpAnimEventHandler : CharacterAnimEventHandler
{
    private Animator _animator;
    private readonly int _aInspectMultiplier = Animator.StringToHash("InspectMultiplier");
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }
    protected void CheckInspectKey()
    {
        if (Input.GetKey(KeyCode.F))
        {
            _animator.SetFloat(_aInspectMultiplier, 0);
        }        
    }
    protected void EndInspect()
    {
        GetComponentInParent<PlayerState>().EndInspect();
    }
    public void ResetInspectMultiplier()
    {
        _animator.SetFloat(_aInspectMultiplier, 1);
    }
}
