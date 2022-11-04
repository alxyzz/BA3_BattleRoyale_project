using Mirror;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Principal;
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

    private void Start()
    {
        if (!isLocalPlayer) return;
        Debug.Log("Local player start!");
        // initial weapon
        WeaponData initData = LevelManager.Instance.initialWeapon;
        PickUpWeapon(new WeaponIdentityData(initData, initData.Ammo, initData.BackupAmmo));
    }

    [Header("Components")]
    [SerializeField] private Transform _tpSocketWeaponRight;
    [SerializeField] private Transform _fpSocketWeaponRight;
    [SerializeField] private Animator _firstPersonAnimator;
    [SerializeField] private Animator _thirdPersonAnimator;
    private readonly int _aFire = Animator.StringToHash("Fire");


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

    public void PickUpWeapon(WeaponIdentityData identity)
    {
        UIManager.SetNewWeapon((int)identity.Data.RangeType, identity.Data.WeaponName);

        // if the range type of picked weapon is the same as the one I currently equipped
        if (currentWeaponIndex == (int)identity.Data.RangeType)
        {
            ThrowWeapon(CurrentWeaponIdentity);
            inventoryWeapons[currentWeaponIndex] = identity;
            EquipAt(currentWeaponIndex);
        }
        // if I don't have a weapon equipped yet
        else if (currentWeaponIndex < 0)
        {
            inventoryWeapons[(int)identity.Data.RangeType] = identity;
            EquipAt((int)identity.Data.RangeType);
        }
        else
        {
            inventoryWeapons[(int)identity.Data.RangeType] = identity;
        }
        // CmdSetCurrentWeapon(CurrentWeaponIdentity.Data.WeaponName);
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="param">0 means button down; 1 means holding button</param>
    public void FireLocal(int param)
    {
        WeaponInHand weapon = _currentWeaponObj.GetComponent<WeaponInHand>();
        if (weapon.CanFire(param))
        {
            _firstPersonAnimator.SetTrigger(_aFire);
            _thirdPersonAnimator.SetTrigger(_aFire);
            weapon.FireLocal();
            UIManager.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);            
            CmdFire(weapon.GetSpread(), weapon.GetMaxRange(), weapon.GetDamage(), weapon.GetName(), nickname);
        }
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
            Debug.Log(CurrentWeaponIdentity.Data.WeaponName);
            _currentWeaponObj.GetComponent<WeaponInHand>().Init(CurrentWeaponIdentity);

            UIManager.ActiveInventorySlot(currentWeaponIndex);
            UIManager.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);
            UIManager.SetBackupAmmo(CurrentWeaponIdentity.BackupAmmo);
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

   



public void EquipPrevious()
{
    int k;
    for (int i = 1; i < inventoryWeapons.Length; i++)
    {
        k = (currentWeaponIndex + inventoryWeapons.Length - i) % inventoryWeapons.Length;
        Debug.Log(k);
        if (inventoryWeapons[k] != null)
        {
            k = (currentWeaponIndex + i) % inventoryWeapons.Length;
            if (inventoryWeapons[k] != null)
            {
                EquipAt(k);
                break;
            }
        }
    }
}
public void EquipNext()
{
    int k;
    for (int i = 1; i < inventoryWeapons.Length; i++)
    {
            k = (currentWeaponIndex + inventoryWeapons.Length - i) % inventoryWeapons.Length;
            if (inventoryWeapons[k] != null)
        {
            EquipAt(k);
            break;
        }
    }
}
}
