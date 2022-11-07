using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponOverworld : NetworkBehaviour, IInteractable
{
    private GameObject _widget;
    [SerializeField] private WeaponData _data;
    public int CurrentAmmo { get; private set; }
    public int BackupAmmo { get; private set; }
    public void SetAmmo(int current, int backup)
    {
        Debug.Log("Weapon Overworld Set Ammo");
        CurrentAmmo = current;
        BackupAmmo = backup;
    }
    private void Awake()
    {
        Debug.Log("Weapon Overworld Awake");
        CurrentAmmo = _data.Ammo;
        BackupAmmo = _data.BackupAmmo;
    }

    public void BeInteracted(PlayerState pState)
    {
        pState.PickUpWeapon(new WeaponIdentityData(_data, CurrentAmmo, BackupAmmo));
        CmdDestroy();
    }


    public void EndBeingSeen()
    {
        // Debug.Log(name + " was unseen.");
        UIManager.ClearInteractionHint();
        Destroy(_widget);
    }

    public void StartBeingSeen()
    {
        Debug.Log(name + " was seen.");
        UIManager.AddInteractionHint("E: Pick up");
        _widget = Instantiate(Resources.Load<GameObject>("UI/InfoWidget"));
        _widget.GetComponent<WID_Info>().Initialise(transform, _data.WeaponName, CurrentAmmo, BackupAmmo, _data.RangeType);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
