using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpAnimEventHandler : CharacterAnimEventHandler
{



    private void FootStep()
    {
        GetComponentInParent<AudioSource>().PlayOneShot(SoundList.GetRandomFootstep());

    }
}
