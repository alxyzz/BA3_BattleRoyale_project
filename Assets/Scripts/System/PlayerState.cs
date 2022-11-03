using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

/* A Player State is the state of a participant in the game.
 * Some examples of player information that the Player State can contain include:
 *   Name
 *   Current level
 *   Health
 *   Score
 * Player States for all players exist on all machines and can replicate data from the server to the client to keep things in sync.
*/ 
public class PlayerState : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Player state start. Net ID : " + netId);
        GameState.PlayerStates.Add(netId, this);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Player state stop. Net ID : " + netId);
        GameState.PlayerStates.Remove(netId);
    }

    private void Awake()
    {
        CurrentWeaponIndex = -1;
    }

    [Header("Components")]
    [SerializeField] private Transform _tpSocketWeaponRight;
    [SerializeField] private Transform _fpSocketWeaponRight;


    [SyncVar][HideInInspector] public string nickname;
    [SyncVar][HideInInspector] public int health;
    [SyncVar][HideInInspector] public int kills;
    [SyncVar(hook = nameof(OnBodyColourChanged))][HideInInspector] public Color bodyColour;
    [SyncVar(hook = nameof(OnCurrentWeaponChanged))][HideInInspector] public string currentWeapon;

    // WeaponRangeType.SHORT = 0 ; WeaponRangeType.MEDIUM = 1 ; WeaponRangeType.LONG = 2
    public int CurrentWeaponIndex { get; private set; }
    public WeaponIdentityData[] inventoryWeapons = new WeaponIdentityData[3];
    private GameObject _currentWeaponObj;

    public void PickUpWeapon(WeaponIdentityData identity)
    {
        if (inventoryWeapons[(int)identity.Data.RangeType] != null)
            ThrowWeapon(inventoryWeapons[(int)identity.Data.RangeType]);
        inventoryWeapons[(int)identity.Data.RangeType] = identity;
        if (CurrentWeaponIndex < 0)
            CurrentWeaponIndex = (int)identity.Data.RangeType;
        CmdSetCurrentWeapon(inventoryWeapons[CurrentWeaponIndex].Data.WeaponName);
        
    }
    public void TryThrowCurrentWeapon()
    {

    }
    public void ThrowWeapon(WeaponIdentityData identity)
    {
        LevelManager.Instance.CmdSpawnWeaponOverworld(
            identity.Data.WeaponName,
            transform.position + Vector3.up + transform.forward,
            identity.CurrentAmmo,
            identity.BackupAmmo);
    }

    [Command]
    public void CmdSetBodyColour(Color colour) { bodyColour = colour; }
    private void OnBodyColourChanged(Color oldColour, Color newColour)
    {
        foreach (var item in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            item.material.color = newColour;
        }        
    }

    [Command]
    public void CmdSetCurrentWeapon(string weapon) { currentWeapon = weapon; }
    private void OnCurrentWeaponChanged(string oldWeapon, string newWeapon)
    {
        if (newWeapon == string.Empty) return;
        if (_currentWeaponObj != null) Destroy(_currentWeaponObj);

        string path = Path.Combine("Weapons", "InHand", newWeapon);
        if (isLocalPlayer)
        {
            _currentWeaponObj = Instantiate(Resources.Load<GameObject>(path), _fpSocketWeaponRight);
            foreach (var item in _currentWeaponObj.GetComponentsInChildren<Renderer>())
            {
                item.shadowCastingMode = ShadowCastingMode.Off;
            }            
        }
        else
        {
            _currentWeaponObj = Instantiate(Resources.Load<GameObject>(path), _tpSocketWeaponRight);
        }
    }
}
