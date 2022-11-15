using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
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

    private WeaponIdentityData _identity;
    private LocalPlayerController _playerCtrl;
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

    public bool IsHolstered { get; set; } = true;
    private bool _isFiring = false;
    private bool _isReloading = false;
    private bool _isInspected = false;
    private float _recoilValue;
    private float RecoilValue
    {
        get { return _recoilValue; }
        set {
            _recoilValue = Mathf.Clamp(value, 1, _identity.Data.Ammo);
        }
    }
    private Quaternion RecoilRot
    {
        get
        {
            return Quaternion.Euler(
            _identity.Data.RecoilVertical.Evaluate(RecoilValue) * -1 * _playerCtrl.CharaMovementComp.RecoilMultiplier,
            _identity.Data.RecoilHorizontal.Evaluate(RecoilValue) * _playerCtrl.CharaMovementComp.RecoilMultiplier,
            0f);
        }
    }
    private Quaternion GetClampedRecoilRot(float pitchClamp)
    {
        return Quaternion.Euler(
                    Mathf.Clamp(_identity.Data.RecoilVertical.Evaluate(RecoilValue) * -1 * _playerCtrl.CharaMovementComp.RecoilMultiplier, pitchClamp, 0),
                    _identity.Data.RecoilHorizontal.Evaluate(RecoilValue) * _playerCtrl.CharaMovementComp.RecoilMultiplier,
                    0f);
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
        float r = Random.Range(0f, _identity.Data.FireSpread) * _playerCtrl.CharaMovementComp.SpreadMultiplier;
        float angle = Random.Range(0, Mathf.PI * 2);
        center.x += Mathf.Cos(angle) * r;
        center.y += Mathf.Sin(angle) * r;
        directions = new List<Vector3>();
        directions.Add(center - Camera.main.transform.position);  
    }

    public virtual void FireBurst(out List<Vector3> directions)
    {
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

    //private Coroutine _coroutineCrosshairRecover;
    //private IEnumerator CrosshairRecover()
    //{
    //    const float duration = 0.15f;
    //    float time = 0.0f;
    //    while (time < duration)
    //    {
    //        time = Mathf.Min(duration, time + Time.deltaTime);
    //        UI_GameHUD.SetCrosshairWeaponSpread(Mathf.Lerp(_identity.Data.FireSpread, _identity.Data.BasicSpread, time / duration));
    //        yield return null;
    //    }
    //}
    private IEnumerator ContinuousFiringDelay()
    {
        yield return new WaitForSeconds(_identity.Data.FireDelay);
        _isFiring = false;
    }
    private Coroutine _cRecoilRecovery;
    private IEnumerator RecoilRecovery()
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
    //private float _cameraRecoilValue;
    //private float _cameraRecoilVelocity;
    //private void Update()
    //{
    //    if (_isFiring)
    //    {
    //        _cameraRecoilValue =
    //        Mathf.SmoothDamp(_cameraRecoilValue, _recoilValue, ref _cameraRecoilVelocity, _identity.Data.FireDelay);

    //        Camera.main.transform.localRotation =
    //        Quaternion.Euler(
    //            _identity.Data.RecoilVertical.Evaluate(_cameraRecoilValue) * -1,
    //            _identity.Data.RecoilHorizontal.Evaluate(_cameraRecoilValue),
    //            0f
    //           );
    //    }
    //}
    public bool CanReload()
    {
        return !IsHolstered && !_isReloading && _identity.CurrentAmmo < _identity.Data.Ammo && _identity.BackupAmmo > 0;
    }
    //public int GetDamage()
    //{
    //    return _identity.Data.DamageBody;

    //}
    public bool CanInspect()
    {
        return !_isInspected && !_isFiring && !_isReloading;
    }
    public void SetInspect(bool inspect)
    {
        _isInspected = inspect;
    }
    public string GetName()
    {
        return _identity.Data.WeaponName;
    }

    public float GetMaxRange()
    {
        try
        {
            switch (_identity.Data.RangeType)
            {
                case WeaponRangeType.SHORT:
                    return 10;

                case WeaponRangeType.MEDIUM:
                    return 20;

                case WeaponRangeType.LONG:
                    return 40;

                default:
                    throw new System.Exception("@ GetMaxRange(): _identity.Data.RangeType has unpredicted state.");

            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            throw;
        }
    }
    public virtual void StartReload()
    {
        FireStop();
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
}
