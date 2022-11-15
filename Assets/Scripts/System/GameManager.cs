using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private void Awake()
    {
        if (null != instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [Header("Weapon")]
    [SerializeField] private List<WeaponData> _wpnDatabase;

    public static WeaponData GetRandomWeaponData()
    {
        return instance._wpnDatabase[Random.Range(0, instance._wpnDatabase.Count)];
    }
    public static WeaponData GetWeaponData(int index)
    {
        return instance._wpnDatabase[index];
    }
    public static int GetWeaponDataIndex(WeaponData data)
    {
        return instance._wpnDatabase.IndexOf(data);
    }
}
