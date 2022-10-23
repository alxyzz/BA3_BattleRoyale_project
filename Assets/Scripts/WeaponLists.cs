using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLists : MonoBehaviour
{
    public List<Weapon> w_melee = new List<Weapon>();
    public List<Weapon> w_short = new List<Weapon>();
    public List<Weapon> w_medium = new List<Weapon>();
    public List<Weapon> w_long = new List<Weapon>();

    public static WeaponLists Instance;

    private void Awake()
    {
        // First time run
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }
}
