using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimEventHandler : MonoBehaviour
{
    // private Animator _animator;
    private PlayerState _playerState;
    private void Awake()
    {
        // _animator = GetComponent<Animator>();
        _playerState = GetComponentInParent<PlayerState>();
    }

    private void OnUnholstered()
    {
        _playerState.OnUnholstered();
    }
    private void ReloadAttachToHand(int attach)
    {
        _playerState.ReloadAttachToHand(attach);
    }
    private void Reload()
    {
        _playerState.Reload();
    }
    private void EndReload()
    {
        _playerState.EndReload();
    }
}
