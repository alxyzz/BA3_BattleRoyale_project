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
}
