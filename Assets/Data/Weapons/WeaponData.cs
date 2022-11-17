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
    //[SerializeField] private float _effectiveRange;
    //public float EffectiveRange => _effectiveRange;
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
    [SerializeField] private float _inAirSpreadGain = 0.4f;
    public float InAirSpreadGain => _inAirSpreadGain;
    [SerializeField] private float _joggingSpreadGain = 0.04f;
    public float JoggingSpreadGain => _joggingSpreadGain;
    [SerializeField] private float _walkingSpreadGain = 0.02f;
    public float WalkingSpreadGain => _walkingSpreadGain;
    [SerializeField] private float _crouchingSpreadGain = 0.01f;
    public float CrouchingSpreadGain => _crouchingSpreadGain;
    [SerializeField] private float _inAirSpreadMultiplier = 3.0f;
    public float InAirSpreadMultiplier => _inAirSpreadMultiplier;
    [SerializeField] private float _joggingSpreadMultiplier = 2.0f;
    public float JoggingSpreadMultiplier => _joggingSpreadMultiplier;
    [SerializeField] private float _walkingSpreadMultiplier = 0.9f;
    public float WalkingSpreadMultiplier => _walkingSpreadMultiplier;
    [SerializeField] private float _crouchingSpreadMultiplier = 0.8f;
    public float CrouchingSpreadMultiplier => _crouchingSpreadMultiplier;


    [Header("Damage")]
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _dmgHeadMultiplier = 3.2f;
    [SerializeField] private float _dmgBodyMultiplier = 0.9f;
    [SerializeField] private float _dmgArmMultiplier = 1.0f;
    [SerializeField] private float _dmgThighMultiplier = 0.7f;
    [SerializeField] private float _dmgCalfMultiplier = 0.6f;
    public float BaseDamage => _baseDamage;
    public float DamageHead => _baseDamage * _dmgHeadMultiplier;
    public float DamageBody => _baseDamage * _dmgBodyMultiplier;
    public float DamageArm => _baseDamage * _dmgArmMultiplier;
    public float DamageThigh => _baseDamage * _dmgThighMultiplier;
    public float DamageCalf => _baseDamage * _dmgCalfMultiplier;
    [SerializeField] private AnimationCurve _distanceAttenuation = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float GetDistanceAttenuation(float distance)
    {
        return _distanceAttenuation.Evaluate(Mathf.Clamp01(distance / MaxRange));        
    }
    [Tooltip("WOOD, STONE, METAL, GLASS, WATER, HUMAN")]
    [SerializeField] private float[] _PenetrationAttenuation = new float[6];
    public float GetPenetrationAttenuation(PenetrableMaterial mat)
    {
        return _PenetrationAttenuation[(int)mat];
    }

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
    [SerializeField] private AudioClip _fireSound;
    public AudioClip FireSound => _fireSound;
    [SerializeField] private Sprite _killIcon;
    public Sprite KillIcon => _killIcon;
}
