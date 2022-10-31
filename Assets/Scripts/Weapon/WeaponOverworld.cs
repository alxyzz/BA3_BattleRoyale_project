using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponOverworld : NetworkBehaviour, IInteractable
{
    [SerializeField] private WeaponData _data;
    public PlayerState Owner { get; protected set; }
    public int CurrentAmmo { get; set; }
    public int BackupAmmo { get; set; }

    public void BeInteracted(PlayerState pState)
    {
        if (Owner == null)
        {
            NetworkServer.Destroy(gameObject);
        }
    }


    public void EndBeingSeen()
    {
        // Debug.Log(name + " was unseen.");
        UIManager.ClearInteractionHint();
    }

    public void StartBeingSeen()
    {
        Debug.Log(name + " was seen.");
        UIManager.AddInteractionHint("E: Pick up");
    }
}
