using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
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
    [SerializeField] private float _effectiveRange;
    public float EffectiveRange => _effectiveRange;
    [SerializeField] private float _maxRange;
    public float MaxRange => _maxRange;

    [Header("Fire")]
    [Tooltip("is the weapon an automatic firearm (be able to hold LMB to fire)")]
    [SerializeField] private bool _automatic;
    public bool IsAutomatic => _automatic;
    [Tooltip("delay for continuous shooting")]
    [SerializeField] private float _fireDelay;
    public float FireDelay => _fireDelay;
    
    //[Tooltip("degrees the raycast will deviate randomly")]
    //[SerializeField] private float _spread;
    //public float Spread => _spread;
    [Header("Recoil")]
    [Tooltip("Bias of degree. Abscissa range from 0 to _ammo")]
    [SerializeField] private AnimationCurve _recoilHorizontal;
    public AnimationCurve RecoilHorizontal => _recoilHorizontal;
    [Tooltip("Bias of degree. Abscissa range from 0 to _ammo")]
    [SerializeField] private AnimationCurve _recoilVertical;
    public AnimationCurve RecoilVertical => _recoilVertical;
    [Tooltip("Recoil recovery duration for certain ammo")]
    [SerializeField] private AnimationCurve _recoilRecoveryDuration = AnimationCurve.EaseInOut(0, 0.3f, 1, 1);
    public AnimationCurve RecoilRecoveryDuration => _recoilRecoveryDuration;
    [Tooltip("how much recoil value should be added when burst shoot quickly.")]
    [SerializeField] private float _burstRecoilGain;
    public float BurstRecoilGain => _burstRecoilGain;

    [Header("Spread")]
    [Tooltip("basic spread on the crosshair. unit is pixel")]
    [SerializeField] private float _crosshairSpread;
    public float CrosshairSpread => _crosshairSpread;
    [Tooltip("spread when firing. unit is meter")]
    [SerializeField] private float _fireSpread;
    public float FireSpread => _fireSpread;

    [Header("Damage")]
    [SerializeField] private int _damageHead;
    public int DamageHead => _damageHead;
    [SerializeField] private int _damageBody;
    public int DamageBody => _damageBody;
    [SerializeField] private int _damageLeg;
    public int DamageLeg => _damageLeg;

    [Header("Reload")]
    [SerializeField] private float _reloadMultiplier;
    public float ReloadMultiplier => _reloadMultiplier;
    [SerializeField] private int _ammo;
    public int Ammo => _ammo;
    [SerializeField] private int _backupAmmo;
    public int BackupAmmo => _backupAmmo;

    [Header("Miscellaneous")]
    [SerializeField] private float _movementMultiplier;
    public float MovementMultiplier => _movementMultiplier;
}
