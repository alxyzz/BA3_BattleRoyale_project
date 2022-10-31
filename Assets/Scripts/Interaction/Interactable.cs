using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void StartBeingSeen();
    public void EndBeingSeen();
    public void BeInteracted(PlayerState pState);
}
