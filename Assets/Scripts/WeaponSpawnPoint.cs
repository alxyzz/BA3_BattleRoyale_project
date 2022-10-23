using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawnPoint : MonoBehaviour
{
    /// <summary>
    /// Spawn one of the following categories of weapons, or just spawn a specific one if one is defined
    /// </summary>
    public enum spawning_type
    {
        shortrange,
        mediumrange,
        longrange,
        melee
    }
    public spawning_type spawnedItem = spawning_type.shortrange;
    [ReadOnly] public string tip = "If we have a specific weapon prefab below, we will be spawning that, otherwise just pick a random one from the categories above.";
    public Weapon spawnedWeaponPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnedWeaponPrefab != null)
        {
            SpawnWeapon(spawnedWeaponPrefab);
        }
        else
        {

        }
    }

    void SpawnWeapon(Weapon b = null)
    {

    }
    
}
