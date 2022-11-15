using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpAnimEventHandler : CharacterAnimEventHandler
{



    private void FootStep()
    {
        GetComponentInParent<LocalPlayerController>().PlayFootStepForEveryone();
    }
}
