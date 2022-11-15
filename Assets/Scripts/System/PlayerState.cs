using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class RaycastHitComparer : IComparer<RaycastHit>
{
    public int Compare(RaycastHit x, RaycastHit y)
    {
        return x.distance.CompareTo(y.distance);
    }

}

/* A Player State is the state of a participant in the game.
 * Some examples of player information that the Player State can contain include:
 *   Name
 *   Current level
 *   Health
 *   Score
 * Player States for all players exist on all machines and can replicate data from the server to the client to keep things in sync.
*/
public class PlayerState : NetworkBehaviour, IDamageable, ISubject
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        health = 100;
        kills = 0;
        ping = -1;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Player state start. Net ID : " + netId);
        GameState.AddPlayer(this);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Player state stop. Net ID : " + netId);
        GameState.RemovePlayer(this);
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetNickname(SteamFriends.GetPersonaName());
        _cUpdatePing = StartCoroutine(UpdatePing());
    }
    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        StopCoroutine(_cUpdatePing);
    }

    private void Awake()
    {
        _charaAnimHandler = GetComponent<CharacterAnimHandler>();
      
    }
    private void Start()
    {
        _observers.Add(GameState.instance);
        if (!isLocalPlayer) return;
        Debug.Log("Local player start!");
    }

    [TargetRpc]
    public void TargetInitialWeapon()
    {
        UI_GameHUD.SetUIEnabled(true);

        GetComponent<LocalPlayerController>().LocalStartGame();
        // initial weapon
        WeaponData initData = LevelManager.Instance.initialWeapon;
        PickUpWeapon(new WeaponIdentityData(initData, initData.Ammo, initData.BackupAmmo));
    }

    [Header("Components")]
    [SerializeField] private Transform _tpSocketWeaponLeft;
    [SerializeField] private Transform _tpSocketWeaponRight;
    [SerializeField] private Transform _fpSocketWeaponLeft;
    [SerializeField] private Transform _fpSocketWeaponRight;
    // [SerializeField] private Animator _firstPersonAnimator;
    // [SerializeField] private Animator _thirdPersonAnimator;
    private List<IObserver> _observers = new List<IObserver>();
    private CharacterAnimHandler _charaAnimHandler;
    private readonly int _aFire = Animator.StringToHash("Fire");
    private readonly int _aReload = Animator.StringToHash("Reload");
    private readonly int _aUnholster = Animator.StringToHash("Unholster");
    private readonly int _aInspect = Animator.StringToHash("Inspect");


    [SyncVar(hook = nameof(OnNicknameChanged))][HideInInspector] public string nickname;
    [SyncVar][HideInInspector] public int health;
    [SyncVar(hook = nameof(OnKillsChanged))][HideInInspector] public int kills;
    [SyncVar(hook = nameof(OnBodyColourChanged))][HideInInspector] public Color bodyColour;

    [SyncVar(hook = nameof(OnPingChanged))][HideInInspector] public int ping;

    // WeaponRangeType.SHORT = 0 ; WeaponRangeType.MEDIUM = 1 ; WeaponRangeType.LONG = 2
    [SyncVar][HideInInspector] public int curWpnIndex = -1;
    [SyncVar(hook = nameof(OnCurWpnDbIndexChanged))][HideInInspector] public int curWpnDbIndex = -1;

    public WeaponIdentityData[] inventoryWeapons = new WeaponIdentityData[3];
    public WeaponIdentityData CurrentWeaponIdentity
    {
        get
        {
            if (curWpnIndex >= 0) return inventoryWeapons[curWpnIndex];
            else return null;
        }
    }
    private GameObject _curWpnObj;
    public WeaponInHand CurrentWeaponInHand
    {
        get
        {
            if (_curWpnObj != null) return _curWpnObj.GetComponent<WeaponInHand>();
            else return null;
        }
    }

    public void PickUpWeapon(WeaponIdentityData identity)
    {
        UI_GameHUD.SetNewWeapon((int)identity.Data.RangeType, identity.Data.WeaponName);

        if (inventoryWeapons[(int)identity.Data.RangeType] != null)
        {
            ThrowWeapon(inventoryWeapons[(int)identity.Data.RangeType]);
        }
        inventoryWeapons[(int)identity.Data.RangeType] = identity;

        // if the range type of picked weapon is the same as the one I currently equipped
        // if I don't have a weapon equipped yet
        if (curWpnIndex < 0 || curWpnIndex == (int)identity.Data.RangeType)
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
            EndInspect();
            _charaAnimHandler.FpSetTrigger(_aFire);
            _charaAnimHandler.CmdTpSetTrigger(_aFire);
            CurrentWeaponInHand.FireBurst(out List<Vector3> directions);
            UI_GameHUD.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);

            CmdFire(curWpnDbIndex, Camera.main.transform.position, directions);
        }

    }
    public void FireContinuously()
    {
        if (CurrentWeaponInHand.CanFireContinuously())
        {
            _charaAnimHandler.FpSetTrigger(_aFire);
            _charaAnimHandler.CmdTpSetTrigger(_aFire);
            CurrentWeaponInHand.FireContinuously(out List<Vector3> directions);
            UI_GameHUD.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);
            CmdFire(curWpnDbIndex, Camera.main.transform.position, directions);
        }
    }
    public void FireStop()
    {
        CurrentWeaponInHand.FireStop();
    }

    [Command]
    private void CmdSetCurWpn(int index, int dbIndex)
    {
        curWpnIndex = index;
        curWpnDbIndex = dbIndex;
    }
    private void OnCurWpnDbIndexChanged(int oldIndex, int newIndex)
    {
        if (_curWpnObj != null) Destroy(_curWpnObj);
        WeaponData data = GameManager.GetWeaponData(newIndex);
        string path = Path.Combine("Weapons", "InHand", data.WeaponName);
        if (isLocalPlayer)
        {
            _curWpnObj = Instantiate(Resources.Load<GameObject>(path), _fpSocketWeaponRight);
            foreach (var item in _curWpnObj.GetComponentsInChildren<Renderer>())
            {
                item.shadowCastingMode = ShadowCastingMode.Off;
            }
            _curWpnObj.GetComponent<WeaponInHand>().Init(CurrentWeaponIdentity, GetComponent<LocalPlayerController>());

            UI_GameHUD.ActiveInventorySlot(curWpnIndex);
            UI_GameHUD.SetAmmo(CurrentWeaponIdentity.CurrentAmmo);
            UI_GameHUD.SetBackupAmmo(CurrentWeaponIdentity.BackupAmmo);
            UI_GameHUD.SetCrosshairWeaponSpread(CurrentWeaponIdentity.Data.CrosshairSpread);
        }
        else
        {
            _curWpnObj = Instantiate(Resources.Load<GameObject>(path), _tpSocketWeaponRight);
        }
    }
    //[Command]
    //private void CmdSetCurWpnName(string newName) { currentWeaponName = newName; }
    public void EquipAt(int index)
    {
        if (inventoryWeapons[index] != null)
        {
            _charaAnimHandler.FpSetTrigger(_aUnholster);
            _charaAnimHandler.CmdTpSetTrigger(_aUnholster);

            CmdSetCurWpn(index, GameManager.GetWeaponDataIndex(inventoryWeapons[index].Data));
        }
    }
    [Command]
    public void CmdFire(int dbIndex, Vector3 origin, List<Vector3> directions)
    {
        WeaponData wpn = GameManager.GetWeaponData(dbIndex);
        RaycastHit[] hits = new RaycastHit[5];
        foreach (Vector3 dir in directions)
        {
            int hitInfoLen = Physics.RaycastNonAlloc(origin, dir, hits, wpn.MaxRange);
            Array.Sort(hits, 0, hitInfoLen, new RaycastHitComparer());
            float attenuation = 1.0f;
            float dmg = 1.0f;
            for (int i = 0; i < hitInfoLen; i++)
            {
                // will not apply damage on self
                PlayerState ps = hits[i].transform.GetComponentInParent<PlayerState>();
                if (this == ps)
                    continue;

                // Apply Damage
                IDamageable d = hits[i].transform.GetComponentInParent<IDamageable>();
                if (null != d)
                {
                    Debug.Log("Get Damageable");
                    if (hits[i].transform.TryGetComponent(out CharacterBodyParts b))
                    {
                        switch (b.Part)
                        {
                            case BodyPart.HEAD: dmg = wpn.DamageHead; break;
                            case BodyPart.BODY: dmg = wpn.DamageBody; break;
                            case BodyPart.ARM: dmg = wpn.DamageArm; break;
                            case BodyPart.THIGH: dmg = wpn.DamageThigh; break;
                            case BodyPart.CALF: dmg = wpn.DamageCalf; break;
                        }
                    }
                    else dmg = wpn.BaseDamage;
                    dmg *= wpn.GetDistanceAttenuation(hits[i].distance);
                    d.ApplyDamage(Mathf.Max(0, Mathf.RoundToInt(dmg * attenuation)), this, null, DamageType.SHOOT);
                }

                // Temperory: spawn decal               
                GameObject obj = Instantiate(Resources.Load<GameObject>("test"), hits[i].point, Quaternion.identity);
                NetworkServer.Spawn(obj);

                // Calculate Attenuation
                if (hits[i].transform.TryGetComponent(out IPenetrable p))
                {
                    attenuation = p.GetAttenuation(attenuation, wpn);
                }
                else
                {
                    // If cannot penetrate, then break
                    break;
                }
            }
        }
    }
    [Command]
    public void GetDamaged(PlayerState who, int damage, string weaponName, PlayerState attacker) //use null for the last two if the damage was environmental
    {
        string log = "";
        bool? b = null;
        if (weaponName != null && weaponName != "" && attacker != null) //not environmental damage)
        {
            b = true;
        }
        log += "Player " + who.nickname + " was damaged for " + damage + " HP by " + b ?? (attacker + "'s " + weaponName + ".") ?? "their own foolishness.";
    }

    public void EquipScroll(int val)
    {
        int k;
        for (int i = 1; i < inventoryWeapons.Length; i++)
        {
            k = (curWpnIndex + inventoryWeapons.Length + val * i) % inventoryWeapons.Length;
            if (inventoryWeapons[k] != null)
            {
                EquipAt(k);
                break;
            }
        }
    }
    public void OnUnholstered()
    {
        if (!IsAlive) return;
        CurrentWeaponInHand.IsHolstered = false;
    }

    public void StartReload()
    {
        if (!IsAlive) return;
        if (CurrentWeaponInHand.CanReload())
        {
            EndInspect();
            _charaAnimHandler.FpSetTrigger(_aReload);
            _charaAnimHandler.CmdTpSetTrigger(_aReload);
            CurrentWeaponInHand.StartReload();
        }
    }
    public void ReloadAttachToHand(int attach)
    {
        if (!IsAlive) return;

        if (attach > 0)
            CurrentWeaponInHand.RemoveMagazine(isLocalPlayer ? _fpSocketWeaponLeft : _tpSocketWeaponLeft);
        else
            CurrentWeaponInHand.LoadMagazine();
    }
    public void Reload()
    {
        if (!IsAlive) return;
        CurrentWeaponInHand.Reload();
    }
    public void EndReload()
    {
        if (!IsAlive) return;
        CurrentWeaponInHand.EndReload();
    }


    #region Damage

    public void ApplyDamage(int amount, PlayerState instigator, GameObject causer, DamageType type)
    {
        Debug.Log($"Current Health : {health} ;;;;;; Applied damage : {amount}");
        health = Mathf.Max(0, health - amount);
        TargetRefreshHealth(health);
        if (health == 0)
        {
            if (instigator != null)
                instigator.CmdAddKill();
            // dead
            RpcDie();
        }
    }
    [TargetRpc]
    public void TargetRefreshHealth(int hp)
    {
        health = hp;
        UI_GameHUD.SetHealth(health);
    }
    #endregion

    #region Statistics
    public Action<string> onNicknameChanged;
    [Command]
    private void CmdSetNickname(string newNickname)
    {
        nickname = newNickname;
    }
    private void OnNicknameChanged(string oldName, string newName)
    {
        onNicknameChanged?.Invoke(newName);
    }
    public Action<int> onPingChanged;
    [Command]
    private void CmdSetPing(int val)
    {
        ping = val;
    }
    private void OnPingChanged(int oldPing, int newPing)
    {
        onPingChanged?.Invoke(newPing);
    }
    Coroutine _cUpdatePing;
    IEnumerator UpdatePing()
    {
        while (true)
        {
            CmdSetPing(Mathf.RoundToInt((float)(NetworkTime.rtt * 1000.0)));
            yield return new WaitForSeconds(1.0f);
        }
    }

    public Action<int> onKillsChanged;
    [Command]
    private void CmdSetKill(int val)
    {
        kills = val;
    }
    private void CmdAddKill()
    {
        kills++;
    }
    private void OnKillsChanged(int oldKills, int newKills)
    {
        onKillsChanged?.Invoke(newKills);
    }

    #endregion

    #region Inspect
    public void Inspect()
    {
        if (CurrentWeaponInHand.CanInspect())
        {
            CurrentWeaponInHand.SetInspect(true);
            _charaAnimHandler.FpSetTrigger(_aInspect);
        }
    }
    public void EndInspect()
    {
        CurrentWeaponInHand.SetInspect(false);
    }
    #endregion
    public bool IsAlive => health > 0;
    public Action onDied;
    [ClientRpc]
    public void RpcDie()
    {
        if (isLocalPlayer)
        {
            _charaAnimHandler.CmdTpSetLayerWeight(1, 0);
            _charaAnimHandler.CmdTpSetTrigger(Animator.StringToHash("Dead"));
            Destroy(_curWpnObj);
            GetComponent<LocalPlayerController>().Die();
            for (int i = 0; i < inventoryWeapons.Length; i++)
            {
                if (null != inventoryWeapons[i])
                {
                    ThrowWeapon(inventoryWeapons[i]);
                }
            }
        }

        onDied?.Invoke();
    }
    void ISubject.Attach(IObserver observer)
    {
        _observers.Add(observer);
    }

    void ISubject.Detach(IObserver observer)
    {
        _observers.Remove(observer);
    }

    
    void ISubject.Notify()
    {
        foreach (IObserver item in _observers)
        {
            Debug.Log("Notified observer: " + item.ToString());
            item.UpdateState(this);
        }
    }
}
