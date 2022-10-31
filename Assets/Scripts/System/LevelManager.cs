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
    [Command(requiresAuthority = false)]
    public void CmdSpawnWeaponOverworld(string weaponName)
    {
        string path = Path.Combine("Weapons", "Overworld", weaponName);
        GameObject obj = Instantiate(Resources.Load<GameObject>(path), new Vector3(0, 3, 0), Quaternion.identity);
        NetworkServer.Spawn(obj);
    }

}
