using Mirror;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Animations;
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

    private void Start()
    {
        if (!isLocalPlayer) return;
        Debug.Log("Local player start!");
        // initial weapon
        WeaponData initData = LevelManager.Instance.initialWeapon;
        PickUpWeapon(new WeaponIdentityData(initData, initData.Ammo, initData.BackupAmmo));
    }

    [Header("Components")]
    [SerializeField] private Transform _tpSocketWeaponLeft;
    [SerializeField] private Transform _tpSocketWeaponRight;
    [SerializeField] private Transform _fpSocketWeaponLeft;
    [SerializeField] private Transform _fpSocketWeaponRight;
    [SerializeField] private Animator _firstPersonAnimator;
    [SerializeField] private Animator _thirdPersonAnimator;
    private readonly int _aFire = Animator.StringToHash("Fire");
    private readonly int _aReload = Animator.StringToHash("Reload");
    private readonly int _aUnholster = Animator.StringToHash("Unholster");


    [SyncVar][HideInInspector] public string nickname;
    [SyncVar][HideInInspector] public int health;
    [SyncVar][HideInInspector] public int kills;
    [SyncVar(hook = nameof(OnBodyColourChanged))][HideInInspector] public Color bodyColour;

    // WeaponRangeType.SHORT = 0 ; WeaponRangeType.MEDIUM = 1 ; WeaponRangeType.LONG = 2
    [SyncVar][HideInInspector] public int currentWeaponIndex = -1;
    [SyncVar(hook = nameof(OnCurWpnNameChanged))][HideInInspector] public string currentWeaponName = "";

    public WeaponIdentityData[] inventoryWeapons = new WeaponIdentityData[3];
    public WeaponIdentityData CurrentWeaponIdentity
    {
        get
        {
            if (currentWeaponIndex >= 0) return inventoryWeapons[currentWeaponIndex];
            else return null;
        }
    }
    private GameObject _currentWeaponObj;
    public WeaponInHand CurrentWeaponInHand
    {
        get
        {
            if (_currentWeaponObj != null) return _currentWeaponObj.GetComponent<WeaponInHand>();
            else return null;
        }
    }

    public void PickUpWeapon(WeaponIdentityData identity)
    {
        UIManager.SetNewWeapon((int)identity.Data.RangeType, identity.Data.WeaponName);

        if (inventoryWeapons[(int)identity.Data.RangeType] != null)
        {
            ThrowWeapon(inventoryWeapons[(int)identity.Data.RangeType]);
        }
        inventoryWeapons[(int)identity.Data.RangeType] = identity;

        // if the range type of picked weapon is the same as the one I currently equipped
        // if I don't have a weapon equipped yet
        if (currentWeaponIndex < 0 || currentWeaponIndex == (int)identity.Data.RangeType)
        {
            EquipAt((int)identity.Data.RangeType);
        }
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

    public void FireBurst()
    {
        if (CurrentWeaponInHand.CanFireBurst())
        {
            _firstPersonAnimator.SetTrigger(_aFire);
            _thirdPersonAnimator.SetTrigger(_aFire);
            CurrentWeaponInHand.FireBurst();
            UIManager.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);
        }

    }
    public void FireContinuously()
    {
        if (CurrentWeaponInHand.CanFireContinuously())
        {
            _firstPersonAnimator.SetTrigger(_aFire);
            _thirdPersonAnimator.SetTrigger(_aFire);
            CurrentWeaponInHand.FireContinuously();
            UIManager.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);
        }
    }
    public void FireStop()
    {
        CurrentWeaponInHand.FireStop();
    }

    [Command]
    private void CmdSetCurWpn(string newName, int index)
    {
        currentWeaponIndex = index;
        currentWeaponName = newName;
    }
    private void OnCurWpnNameChanged(string oldName, string newName)
    {
        if (_currentWeaponObj != null) Destroy(_currentWeaponObj);

        string path = Path.Combine("Weapons", "InHand", newName);
        if (isLocalPlayer)
        {
            _currentWeaponObj = Instantiate(Resources.Load<GameObject>(path), _fpSocketWeaponRight);
            foreach (var item in _currentWeaponObj.GetComponentsInChildren<Renderer>())
            {
                item.shadowCastingMode = ShadowCastingMode.Off;
            }
            _currentWeaponObj.GetComponent<WeaponInHand>().Init(CurrentWeaponIdentity, GetComponent<LocalPlayerController>());

            UIManager.ActiveInventorySlot(currentWeaponIndex);
            UIManager.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);
            UIManager.SetBackupAmmo(CurrentWeaponIdentity.BackupAmmo);
            UIManager.SetCrosshairWeaponSpread(CurrentWeaponIdentity.Data.CrosshairSpread);
        }
        else
        {
            _currentWeaponObj = Instantiate(Resources.Load<GameObject>(path), _tpSocketWeaponRight);
        }
    }
    [Command]
    private void CmdSetCurWpnName(string newName) { currentWeaponName = newName; }
    public void EquipAt(int index)
    {
        if (inventoryWeapons[index] != null)
        {
            _firstPersonAnimator.SetTrigger(_aUnholster);
            _thirdPersonAnimator.SetTrigger(_aUnholster);
            CmdSetCurWpn(inventoryWeapons[index].Data.WeaponName, index);
        }
    }
    [Command]
    public void CmdFire(float spread, float maxRange, int damage, string weaponName, string attacker)
    {

        RaycastHit[] results = new RaycastHit[10];
        Transform play = LocalGame.LocalPlayer.transform;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        //spread
        Vector3 deviation3D = Random.insideUnitCircle * spread;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward * maxRange + deviation3D);
        Vector3 forwardVector = Camera.main.transform.rotation * rot * Vector3.forward;
        if (Application.isEditor)
        {
            Debug.DrawRay(play.position, forwardVector, Color.green);
        }
        Ray ray = new Ray(play.position, forwardVector);
        int hits = Physics.RaycastNonAlloc(ray, results);
        System.Array.Sort(results, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
        //todo - check for cover penetration here, for now only the first hit object
        if (results[0].transform != null)
        {
            if (results[0].transform.tag == "Player")
            {
                try
                {
                    results[0].transform.GetComponent<PlayerBody>().GetDamaged(damage, weaponName);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                    throw;
                }

            }

            for (int i = 0; i < hits; i++)
            {
                Debug.Log("Weapon hits, in chronological order: " + i + " -> " + results[i].transform.gameObject);
            }
        }

    }

    public void EquipScroll(int val)
    {
        int k;
        for (int i = 1; i < inventoryWeapons.Length; i++)
        {
            k = (currentWeaponIndex + inventoryWeapons.Length + val * i) % inventoryWeapons.Length;
            if (inventoryWeapons[k] != null)
            {
                EquipAt(k);
                break;
            }
        }
    }
    public void OnUnholstered()
    {
        CurrentWeaponInHand.IsHolstered = false;
    }

    public void StartReload()
    {
        if (CurrentWeaponInHand.CanReload())
        {
            _firstPersonAnimator.SetTrigger(_aReload);
            _thirdPersonAnimator.SetTrigger(_aReload);
            CurrentWeaponInHand.StartReload();
        }
    }
    public void ReloadAttachToHand(int attach)
    {
        if (attach > 0)
            CurrentWeaponInHand.RemoveMagazine(isLocalPlayer ? _fpSocketWeaponLeft : _tpSocketWeaponLeft);
        else
            CurrentWeaponInHand.LoadMagazine();
    }
    public void Reload()
    {
        CurrentWeaponInHand.Reload();
    }
    public void EndReload()
    {
        CurrentWeaponInHand.EndReload();
    }
}
