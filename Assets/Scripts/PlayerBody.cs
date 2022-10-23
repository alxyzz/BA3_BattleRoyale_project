using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : NetworkBehaviour
{
    //to be added on the player prefab.
    public PlayerData player;
    public float health = 100; 

    public GameObject CameraMountPoint; //to allow switching cameras after respawning without creating new ones - we will reuse the same camera, mounted to an empty game object point on the player entity
    // Start is called before the first frame update


    public Weapon w_melee;
    public Weapon w_short;
    public Weapon w_medium;
    public Weapon w_long;
    public bool canShoot;
    public bool isDead;
    public bool canWalk;

    public Weapon heldWeapon;
    public PlayerBody lastAttackedBy;

    public void GetDamaged(int damage, string damageSource ="", PlayerBody attacker = null)
    {
        health -= damage;
    }


    public void RefreshHealthState()
    {

    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PickupWeapon(Weapon w)
    {
        switch (w.type)
        {
            case Weapon.weapon_type.t_melee:
                w_melee = w;
                break;
            case Weapon.weapon_type.t_short:
                w_short = w;
                break;
            case Weapon.weapon_type.t_medium:
                w_medium = w;
                break;
            case Weapon.weapon_type.t_long:
                w_long = w;
                break;
            default:
                break;
        }
    }

    IEnumerator SwitchWeapon(Weapon target)
    {
        if (heldWeapon != null)
        {
            heldWeapon.anim.SetBool("puttingDown", true);
            yield return new WaitForSeconds(heldWeapon.weaponSwitchSpeed);
            heldWeapon.anim.SetBool("puttingDown", false);
            heldWeapon = target;
            heldWeapon.anim.SetBool("pullingOut", true);
        }


        heldWeapon = target;

        heldWeapon.anim.SetBool("PullOutWeapon", true);

        yield return new WaitForSeconds(heldWeapon.weaponSwitchSpeed);

        heldWeapon.anim.SetBool("PullOutWeapon", false);
        //this is where the other weapon should take over
    }

    public void ResetCamera()
    {
        Transform cameraTransform = Camera.main.gameObject.transform;  //Find main camera which is part of the scene instead of the prefab
        cameraTransform.parent = CameraMountPoint.transform;  //Make the camera a child of the mount point
        cameraTransform.position = CameraMountPoint.transform.position;  //Set position/rotation same as the mount point
        cameraTransform.rotation = CameraMountPoint.transform.rotation;

    }



}
