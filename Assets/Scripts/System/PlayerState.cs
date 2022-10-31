using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A Player State is the state of a participant in the game.
 * Some examples of player information that the Player State can contain include:
 *   Name
 *   Current level
 *   Health
 *   Score
 * Player States for all players exist on all machines and can replicate data from the server to the client to keep things in sync.
*/ 
public class PlayerState : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Player state start. Net ID : " + netId);
        GameState.PlayerStates.Add(netId, this);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Player state stop. Net ID : " + netId);
        GameState.PlayerStates.Remove(netId);
    }

    [SyncVar][HideInInspector] public string nickname;
    [SyncVar][HideInInspector] public int health;
    [SyncVar][HideInInspector] public int kills;

    [SyncVar(hook = nameof(OnBodyColourChanged))][HideInInspector] public Color bodyColour;
    [Header("Inventory")]
    [SyncVar] [HideInInspector] public Weapon currentWeapon;
    public GameObject startingWeaponPrefab;
    [SyncVar] public Weapon w_melee;
    [SyncVar] public Weapon w_short;
    [SyncVar] public Weapon w_medium;
    [SyncVar] public Weapon w_long;
    [SyncVar]public Weapon heldWeapon;
    public PlayerBody lastAttackedBy;



    [Command]
    public void PickupWeapon(Weapon w)
    {
        w.gameObject.SetActive(false);//Makes the weapon disappear. Calling functions on deactivated objects is fine. They just won't update on their own. We can reset the gameobject position upon dropping the weapon and make it visible.
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

        heldWeapon.anim.SetBool("pullingOut", true);

        yield return new WaitForSeconds(heldWeapon.weaponSwitchSpeed);

        heldWeapon.anim.SetBool("pullingOut", false);
        //this is where the other weapon should take over
    }
    [Command]
    public void GetDamaged(int damage, string damageSource, PlayerState attacker) //Command doesn't permit for optional arguments, sadly... Just use null instead if it's environmental damage
    {
        health -= damage;
        string msg = "";
        msg = nickname + " got damaged " + damage + " points by " + damageSource ?? damageSource + "." ?? "an unknown damage source.";
        if (attacker != null)
        {
            msg += "Attacker was " + attacker.nickname;
        }
        //Update health UI here

        Debug.Log(msg);
    }

    [Command]
    public void CmdSetBodyColour(Color colour) { bodyColour = colour; }
    private void OnBodyColourChanged(Color oldColour, Color newColour)
    {
        foreach (var item in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            item.material.color = newColour;
        }        
    }
}
