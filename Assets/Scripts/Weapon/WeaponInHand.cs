using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class WeaponIdentityData
{
    public WeaponIdentityData(WeaponData data, int currentAmmo, int backupAmmo)
    {
        Data = data;
        CurrentAmmo = currentAmmo;
        BackupAmmo = backupAmmo;
    }

    public WeaponData Data { get; set; }
    public int CurrentAmmo { get; set; }
    public int BackupAmmo { get; set; }
}
public class WeaponInHand : MonoBehaviour
{
    [SerializeField] private Transform _magazine;
    private Transform _magazineOriginalParent;
    private Vector3 _magazinOriginalPosition;
    private Quaternion _magazinOriginalRotation;
    public void LoadMagazine()
    {
        _magazine.SetParent(_magazineOriginalParent);
        _magazine.SetLocalPositionAndRotation(_magazinOriginalPosition, _magazinOriginalRotation);
    }
    public void RemoveMagazine(Transform hand)
    {
        _magazine.SetParent(hand);
    }

    [SerializeField] private Transform _muzzle;
    public Transform Muzzle => _muzzle;

    protected WeaponIdentityData _identity;
    public WeaponIdentityData Identity => _identity;
    protected LocalPlayerController _playerCtrl;
    public void Init(WeaponIdentityData identity, LocalPlayerController controller)
    {
        _identity = identity;
        _playerCtrl = controller;
    }
    private void Awake()
    {
        _magazineOriginalParent = _magazine.parent;
        _magazinOriginalPosition = _magazine.localPosition;
        _magazinOriginalRotation = _magazine.localRotation;
    }
    //protected virtual void Start()
    //{
    //    if (_identity == null || _playerCtrl == null)
    //        return;
    //    SetThingsByScopeLevel(0);
    //}
    public bool IsHolstered { get; set; } = true;
    protected bool _isFiring = false;
    protected bool _isReloading = false;
    protected bool _isInspected = false;
    protected float _recoilValue;
    protected float RecoilValue
    {
        get { return _recoilValue; }
        set
        {
            _recoilValue = Mathf.Clamp(value, 1, _identity.Data.Ammo);
        }
    }
    private Quaternion RecoilRot
    {
        get
        {
            return Quaternion.AngleAxis(_identity.Data.RecoilHorizontal.Evaluate(RecoilValue) * _playerCtrl.CharaMovementComp.RecoilMultiplier,
                Vector3.up) * Quaternion.AngleAxis(_identity.Data.RecoilVertical.Evaluate(RecoilValue) * -1 * _playerCtrl.CharaMovementComp.RecoilMultiplier,
                _playerCtrl.FirstPersonRight);
        }
    }
    protected Quaternion GetClampedRecoilRot(float pitchClamp)
    {
        return Quaternion.Euler(
            Mathf.Clamp(_identity.Data.RecoilVertical.Evaluate(RecoilValue) * -1 * _playerCtrl.CharaMovementComp.RecoilMultiplier, pitchClamp, 0),
            _identity.Data.RecoilHorizontal.Evaluate(RecoilValue) * _playerCtrl.CharaMovementComp.RecoilMultiplier,
            0.0f);
    }
    protected virtual float FireSpreadRadius => GetFireSpreadRadius(_identity.Data.FireSpread);
    protected float GetFireSpreadRadius(float baseSpread)
    {
        float gain = 0.0f;
        float multiplier = 1.0f;
        if (!_playerCtrl.CharaMovementComp.IsOnGround)
        {
            gain = _identity.Data.InAirSpreadGain;
            multiplier = _identity.Data.InAirSpreadMultiplier;
        }
        else if (_playerCtrl.CharaMovementComp.LastMovementInput.sqrMagnitude != 0)
        {
            if (_playerCtrl.CharaMovementComp.IsCrouching)
            {
                gain = _identity.Data.CrouchingSpreadGain;
                multiplier = _identity.Data.CrouchingSpreadMultiplier;
            }
            else if (_playerCtrl.CharaMovementComp.IsWalking)
            {
                gain = _identity.Data.WalkingSpreadGain;
                multiplier = _identity.Data.WalkingSpreadMultiplier;
            }
            else
            {
                gain = _identity.Data.JoggingSpreadGain;
                multiplier = _identity.Data.JoggingSpreadMultiplier;
            }
        }
        return (baseSpread + gain) * multiplier;
    }
    public virtual bool CanFireBurst()
    {
        if (_identity.CurrentAmmo <= 0)
        {
            // do something : cannot fire burst because current ammo is not enough
            FireStop();
            return false;
        }
        if (IsHolstered)
        {
            return false;
        }
        if (_isReloading)
        {
            // do something : cannot fire burst because of _isReloading
            return false;
        }
        if (_isFiring)
        {
            //  do something : cannot fire burst because of _isFiring;
            return false;
        }
        return true;
    }
    public virtual bool CanFireContinuously()
    {
        if (!_identity.Data.IsAutomatic)
        {
            return false;
        }
        return CanFireBurst();
    }

    protected virtual void HitScan(out List<Vector3> directions)
    {
        // RaycastHit[] hits = new RaycastHit[10];
        Vector3 dir = RecoilRot * _playerCtrl.FirstPersonForward;
        Vector3 center = Camera.main.transform.position + dir;
        float r = Random.Range(0f, FireSpreadRadius);
        float angle = Random.Range(0, Mathf.PI * 2);
        center.x += Mathf.Cos(angle) * r;
        center.y += Mathf.Sin(angle) * r;
        directions = new List<Vector3>();
        directions.Add(center - Camera.main.transform.position);
    }

    public virtual void FireBurst(out List<Vector3> directions)
    {
        SetInspect(false);
        _identity.CurrentAmmo--;
        _isFiring = true;
        if (_cRecoilRecovery != null) StopCoroutine(_cRecoilRecovery);

        // 根据当前 _recoilValue 计算受后坐力影响得到的偏移射线
        HitScan(out directions);

        if (RecoilValue == 0)
        {
            RecoilValue = 1;
        }
        else
        {
            RecoilValue += _identity.Data.BurstRecoilGain;
        }

        //Camera.main.transform.localRotation = GetClampedRecoilRot(-5);
        Camera.main.GetComponent<CameraShake>().ShakeTo(
            GetClampedRecoilRot(-5),
            _identity.Data.FireDelay,
            _identity.Data.RecoilRecoveryDuration.Evaluate(RecoilValue / _identity.Data.Ammo));
        UI_GameHUD.SetCrosshairFireSpread(_identity.Data.CrosshairSpread * 2.0f, _identity.Data.FireDelay);
        StartCoroutine(ContinuousFiringDelay());
    }

    public virtual void FireContinuously(out List<Vector3> directions)
    {
        _identity.CurrentAmmo--;
        _isFiring = true;

        // 根据当前 _recoilValue 计算受后坐力影响得到的偏移射线
        HitScan(out directions);

        RecoilValue += 1;

        Camera.main.GetComponent<CameraShake>().ShakeTo(
            GetClampedRecoilRot(-5),
            _identity.Data.FireDelay,
            _identity.Data.RecoilRecoveryDuration.Evaluate(RecoilValue / _identity.Data.Ammo));
        UI_GameHUD.SetCrosshairFireSpread(_identity.Data.CrosshairSpread * 2.0f, _identity.Data.FireDelay);
        StartCoroutine(ContinuousFiringDelay());
    }


    public virtual void FireStop()
    {
        if (!_isFiring) return;
        _isFiring = false;
        // Camera.main.GetComponent<CameraShake>().Stop();
        _cRecoilRecovery = StartCoroutine(RecoilRecovery());
    }
    private IEnumerator ContinuousFiringDelay()
    {
        yield return new WaitForSeconds(_identity.Data.FireDelay);
        _isFiring = false;
    }
    protected Coroutine _cRecoilRecovery;
    protected IEnumerator RecoilRecovery()
    {
        float startValue = _recoilValue;
        float speed = startValue / _identity.Data.RecoilRecoveryDuration.Evaluate(startValue / _identity.Data.Ammo);
        Quaternion startRot = Camera.main.transform.localRotation;
        while (_recoilValue > 0)
        {
            _recoilValue = Mathf.Max(0, _recoilValue - speed * Time.deltaTime);
            //Camera.main.transform.localRotation = Quaternion.Slerp(
            //    Quaternion.identity,
            //    startRot,
            //    _recoilValue / startValue
            //    );
            yield return null;
        }
    }
    public virtual bool CanToggleScope()
    {
        return false;
    }
    public virtual void ToggleScope()
    {
        Debug.Log("Toggle Scope!");
    }
    public virtual void SetThingsByScopeLevel(int level)
    {
        UI_GameHUD.SetScopeActive(level != 0);
        _playerCtrl.MouseSensitivityMultiplier = 1.0f / Mathf.Pow(3, level);
        _playerCtrl.SetFirstPersonVisible(level == 0);
        SetVisible(level == 0);
        Camera.main.fieldOfView = level == 0 ? 60 : (level == 1 ? 10 : 1);
    }
    public bool CanReload()
    {
        return !IsHolstered && !_isReloading && _identity.CurrentAmmo < _identity.Data.Ammo && _identity.BackupAmmo > 0;
    }
    public bool CanInspect()
    {
        return !_isInspected && !_isFiring && !_isReloading;
    }
    public virtual void SetInspect(bool inspect)
    {
        _isInspected = inspect;
    }

    public virtual void StartReload()
    {
        FireStop();
        SetInspect(false);
        _isReloading = true;
    }
    public virtual void Reload()
    {
        int val = Mathf.Min(_identity.BackupAmmo, _identity.Data.Ammo - _identity.CurrentAmmo);
        _identity.CurrentAmmo += val;
        _identity.BackupAmmo -= val;
        UI_GameHUD.SetAmmo(_identity.CurrentAmmo);
        UI_GameHUD.SetBackupAmmo(_identity.BackupAmmo);
    }
    public virtual void EndReload()
    {
        _isReloading = false;
    }
    public virtual void SetVisible(bool visible)
    {
        foreach (var item in GetComponentsInChildren<Renderer>())
        {
            item.gameObject.layer = visible ? LayerMask.NameToLayer("Ignore Raycast") : LayerMask.NameToLayer("Disable Rendering");
        }
    }
}
