using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public Animator anim;
    public string weaponName;
    public float spread;//degrees the raycast will deviate randomly
    public float recoil;//degrees your view gets pushed up for each bullet
    public float damage;
    public float reloadTime;
    public float fireDelay;//delay between firing
    public int currentMagazine;
    public int currentAmmo;
    public bool isReloading;
    public int bulletsInMagazine;

    public float weaponSwitchSpeed;

    private void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    public enum firing_mechanism
    {
        Semi,
        Auto,
        Burst
    }
    public firing_mechanism type_firing;
    public enum weapon_type
    {
        t_melee,
        t_short,
        t_medium,
        t_long
    }

    public weapon_type type;
    IEnumerator Reload()
     {
         isReloading = true;

        anim.SetBool("Reloading", true);
        
         yield return new WaitForSeconds(reloadTime); //interrupt this if we change weapons

        anim.SetBool("Reloading", false);

        currentAmmo -= bulletsInMagazine - currentMagazine;
        currentMagazine = bulletsInMagazine;
        
        isReloading = false;

     }
 



    public void Fire(Transform loc)
    {
        
    }
    
}
