using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    // public static void SpawnWeapon(GameObject pfb)
    private void Start()
    {
        if (isServer)
        {
            if (null == _weaponSpawnerParent) return;
            for (int i = 0; i < _weaponSpawnerParent.childCount; i++)
            {
                WeaponData data = GameManager.GetRandomWeaponData();
                CmdSpawnWeaponOverworld(
                    data.WeaponName,
                    _weaponSpawnerParent.GetChild(i).position,
                    data.Ammo,
                    data.BackupAmmo);
            }
        }
    }

    [Header("Initialise")]
    public WeaponData initialWeapon;
    [SerializeField] private Transform _weaponSpawnerParent;

    [Command(requiresAuthority = false)]
    public void CmdSpawnWeaponOverworld(string weaponName, Vector3 position, int currentAmmo, int backupAmmo)
    {
        string path = Path.Combine("Weapons", "Overworld", weaponName);
        GameObject obj = Instantiate(Resources.Load<GameObject>(path), position, Quaternion.identity);
        obj.GetComponent<WeaponOverworld>().SetAmmo(currentAmmo, backupAmmo);
        NetworkServer.Spawn(obj);
    }

    
}
