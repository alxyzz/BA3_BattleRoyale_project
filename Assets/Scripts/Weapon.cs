using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [HideInInspector]
    public Animator anim;
    public string weaponName;
    private float spread; //degrees the bullet deviates by the end of maximumWeaponRange
    private float recoil;//degrees your view gets pushed up for each bullet
    [SyncVar] public float damage;
    private float reloadDelay;
    private float fireDelay;//delay between firing
    [SyncVar] public int currentMagazine;
    [SyncVar] public int currentAmmo;
    [SyncVar] public bool isReloading;
    private int bulletsInMagazine;
    private int maximumWeaponRange;
    public float weaponSwitchSpeed;

    
    public enum Weapon_State
    {
        Idle,
        Shooting,
        Cooldown,
        Reloading,
        Switching
    }
    [SyncVar]
    public Weapon_State weapon_state = Weapon_State.Idle;
  
    public enum firing_mechanism
    {
        Semi,
        Auto,
        Burst
    }
    [SyncVar]
    public firing_mechanism type_firing;
    public enum weapon_type
    {
        t_melee,
        t_short,
        t_medium,
        t_long
    }
    [SyncVar]
    public weapon_type type;



    private void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    IEnumerator Reload()
     {
         isReloading = true;

        anim.SetBool("Reloading", true);
        
         yield return new WaitForSeconds(reloadDelay); //interrupt this if we change weapons

        anim.SetBool("Reloading", false);

        currentAmmo -= bulletsInMagazine - currentMagazine;
        currentMagazine = bulletsInMagazine;
        
        isReloading = false;

     }


    RaycastHit[] results = new RaycastHit[10];
    [Command]
    public void TryFire()
    {
        if (weapon_state != Weapon_State.Idle)
        {
            //play click sound
            return;
        }
        Transform play = LocalGame.LocalPlayer.transform;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        //spread
        Vector3 deviation3D = Random.insideUnitCircle * spread;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward * maximumWeaponRange + deviation3D);
        Vector3 forwardVector = Camera.main.transform.rotation * rot * Vector3.forward;

        Debug.DrawRay(play.position, forwardVector, Color.green);
        Ray ray = new Ray(play.position, forwardVector);
        int hits = Physics.RaycastNonAlloc(ray, results);
        System.Array.Sort(results, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
        //todo - check for cover penetration here, for now only the first hit object
        if (results[0].transform.tag == "Player")
        {
            results[0].transform.GetComponent<PlayerBody>().GetDamaged(damage, weaponName);
        }

        for (int i = 0; i < hits; i++)
        {
           Debug.Log(results[i].transform.gameObject);
            
        }
        weapon_state = Weapon_State.Cooldown;
        StartCoroutine(coolDown());

    }

    IEnumerator coolDown()
    {

        yield return new WaitForSecondsRealtime(fireDelay);
        if (weapon_state == Weapon_State.Cooldown)
        {
            weapon_state = Weapon_State.Idle;
        }
        
    }
    
}
