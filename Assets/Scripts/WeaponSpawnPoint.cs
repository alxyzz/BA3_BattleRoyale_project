using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponSpawnPoint : MonoBehaviour
{
    /// <summary>
    /// Spawn one of the following categories of weapons, or just spawn a specific one if one is defined
    /// </summary>
    /// 

    public List<GameObject> shortWeapons = new();
    public List<GameObject> mediumWeapons = new();
    public List<GameObject> longWeapons = new();


    public enum spawning_type
    {
        shortrange,
        mediumrange,
        longrange,
    }
    public spawning_type spawnedItem = spawning_type.shortrange;
    //[ReadOnly] public string tip = "If we have a specific weapon prefab below, we will be spawning that, otherwise just pick a random one from the categories above.";

    [Tooltip("If we have a specific weapon prefab below,\nwe will be spawning that, otherwise just pick a random one from the categories above.")]
    public GameObject specificSpawnedWeaponPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (specificSpawnedWeaponPrefab != null)
        {
            SpawnWeapon(specificSpawnedWeaponPrefab);
            
        }
        else
        {
            switch (spawnedItem)
            {
                case spawning_type.shortrange:
                    SpawnWeapon(shortWeapons[Random.Range(0, shortWeapons.Count - 1)]);
                    break;
                case spawning_type.mediumrange:
                    SpawnWeapon(mediumWeapons[Random.Range(0, mediumWeapons.Count - 1)]);
                    break;
                case spawning_type.longrange:
                    SpawnWeapon(longWeapons[Random.Range(0, longWeapons.Count - 1)]);
                    break;
                default:
                    break;
            }
        }
    }

    void SpawnWeapon(GameObject b)
    {
        //spawn the weapon pickup in that location.
    }
    
}
