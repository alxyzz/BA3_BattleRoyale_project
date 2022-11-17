using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpAnimEventHandler : CharacterAnimEventHandler
{
    private AudioSource _audioSource;
    private CharacterMovement _charaMovement;
    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
        _charaMovement = GetComponentInParent<CharacterMovement>();
    }

    private void FootStep()
    {
        if (!_charaMovement.IsWalking)            
        {
            _audioSource.PlayOneShot(SoundList.GetRandomFootstep());
        }
        // GetComponentInParent<LocalPlayerController>().NotifyServerOfFootstep();
    }
}
