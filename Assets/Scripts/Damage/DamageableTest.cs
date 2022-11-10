using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableTest : MonoBehaviour, IDamageable
{
    public void ApplyDamage(int amount, PlayerState instigator, GameObject causer, DamageType type)
    {
        Debug.Log(" ‹µΩ¡À…À∫¶£°" + amount);
    }
}
