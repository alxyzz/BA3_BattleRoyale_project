using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimEventHandler : MonoBehaviour
{
    // private Animator _animator;
    protected PlayerState _playerState;
    protected virtual void Awake()
    {
        // _animator = GetComponent<Animator>();
        _playerState = GetComponentInParent<PlayerState>();
    }

    protected void OnUnholstered()
    {
        _playerState.OnUnholstered();
    }
    protected void ReloadAttachToHand(int attach)
    {
        _playerState.ReloadAttachToHand(attach);
    }
    protected void Reload()
    {
        _playerState.Reload();
    }
    protected void EndReload()
    {
        _playerState.EndReload();
    }
}
