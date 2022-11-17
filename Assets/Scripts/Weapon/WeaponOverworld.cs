using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponOverworld : NetworkBehaviour, IInteractable
{
    private GameObject _widget;
    [SerializeField] private WeaponData _data;

    private GameObject _pfbInfoWidget;
    public int CurrentAmmo { get; private set; }
    public int BackupAmmo { get; private set; }
    public void SetAmmo(int current, int backup)
    {
        CurrentAmmo = current;
        BackupAmmo = backup;
    }
    private void Awake()
    {
        CurrentAmmo = _data.Ammo;
        BackupAmmo = _data.BackupAmmo;
        _pfbInfoWidget = Resources.Load<GameObject>("UI/Game/InfoWidget");
    }

    public void BeInteracted(PlayerState pState)
    {
        pState.PickUpWeapon(_data, CurrentAmmo, BackupAmmo);
        CmdDestroy();
    }


    public void EndBeingSeen()
    {
        // Debug.Log(name + " was unseen.");
        UI_GameHUD.ClearInteractionHint();
        Destroy(_widget);
    }

    public void StartBeingSeen()
    {
        Debug.Log(name + " was seen.");
        UI_GameHUD.AddInteractionHint("E: Pick up");
        _widget = Instantiate(_pfbInfoWidget);
        _widget.GetComponent<WID_Info>().Initialise(transform, _data.WeaponName, CurrentAmmo, BackupAmmo, _data.RangeType);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
