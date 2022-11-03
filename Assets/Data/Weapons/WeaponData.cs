using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    RIFLE,
    PISTOL,
    SHOTGUN,
    SNIPER
}
public enum WeaponRangeType
{
    SHORT,
    MEDIUM,
    LONG
}
[CreateAssetMenu(fileName = "New Weapon", menuName = "Data/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string _weaponName;
    public string WeaponName => _weaponName;
    [SerializeField] private WeaponType _type;
    public WeaponType Type => _type;
    [SerializeField] private WeaponRangeType _rangeType;
    public WeaponRangeType RangeType => _rangeType;

    [Header("Fire")]
    [Tooltip("is the weapon an automatic firearm (be able to hold LMB to fire)")]
    [SerializeField] private bool _automatic;
    public bool IsAutomatic => _automatic;
    [Tooltip("delay between firing")]
    [SerializeField] private float _fireDelay;
    public float FireDelay => _fireDelay;
    [Tooltip("degrees the raycast will deviate randomly")]
    [SerializeField] private float _spread;
    public float Spread => _spread;
    [Tooltip("degrees your view gets pushed up for each bullet")]
    [SerializeField] private float _recoil;
    public float Recoil => _recoil;

    [Header("Damage")]
    [SerializeField] private int _damageHead;
    public int DamageHead => _damageHead;
    [SerializeField] private int _damageBody;
    public int DamageBody => _damageBody;
    [SerializeField] private int _damageLeg;
    public int DamageLeg => _damageLeg;

    [Header("Reload")]
    [SerializeField] private float _reloadDuration;
    public float ReloadDuration => _reloadDuration;
    [SerializeField] private int _ammo;
    public int Ammo => _ammo;
    [SerializeField] private int _backupAmmo;
    public int BackupAmmo => _backupAmmo;

    [Header("miscellaneous")]
    [SerializeField] private float _movementMultiplier;
    public float MovementMultiplier => _movementMultiplier;
}
