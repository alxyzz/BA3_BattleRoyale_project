using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private ParentConstraint _magazine;
    public ParentConstraint Magazine => _magazine;
    [SerializeField] private Transform _muzzle;
    public Transform Muzzle => _muzzle;

    private WeaponIdentityData _identity;
    public void Init(WeaponIdentityData identity)
    {
        _identity = identity;
    }

    private bool _canFire = true;
    private bool _isReloading = false;

    public bool CanFire(int param)
    {
        if (!_identity.Data.IsAutomatic && param != 0)
            return false;
        return _canFire && !_isReloading && _identity.CurrentAmmo > 0;
    }
    public virtual void FireLocal()
    {
        _identity.CurrentAmmo--;
        _canFire = false;
        StartCoroutine(FireRecover());
    }
    private IEnumerator FireRecover()
    {
        yield return new WaitForSeconds(_identity.Data.FireDelay);
        _canFire = true;
    }
    public bool CanReload()
    {
        return _identity.BackupAmmo > 0;
    }
    public virtual void StartReload()
    {
        _isReloading = true;
    }
    public virtual void Reload()
    {
        int val = Mathf.Min(_identity.BackupAmmo, _identity.Data.Ammo - _identity.CurrentAmmo);
        _identity.CurrentAmmo += val;
        _identity.BackupAmmo -= val;
        UIManager.SetAmmo(_identity.CurrentAmmo);
        UIManager.SetBackupAmmo(_identity.BackupAmmo);
    }
    public virtual void EndReload()
    {
        _isReloading = false;
    }
}
