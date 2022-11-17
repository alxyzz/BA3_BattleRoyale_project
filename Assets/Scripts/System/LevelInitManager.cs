using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// this object is on the server only
public class LevelInitManager : NetworkBehaviour
{
    private void Awake()
    {
        if (SteamLobby.Instance == null)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }
    }

    [Header("Weapon Spawning")]
    [SerializeField] private Transform _weaponSpawnerParent;
    private void Start()
    {
        // spawn Weapons
        if (null != _weaponSpawnerParent)
        {
            for (int i = 0; i < _weaponSpawnerParent.childCount; i++)
            {
                WeaponData data = GameManager.GetRandomWeaponData();
                LevelManager.Instance.CmdSpawnWeaponOverworld(
                    data.WeaponName,
                    _weaponSpawnerParent.GetChild(i).position,
                    data.Ammo,
                    data.BackupAmmo);
            }
        }
    }
}
