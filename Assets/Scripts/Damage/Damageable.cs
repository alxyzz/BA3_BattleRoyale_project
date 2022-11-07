using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum DamageType
{
    DEFAULT,
    SHOOT,
    EXPLOSION,
    FALL,
    POISON
}
public interface IDamageable
{
    /// <summary>
    /// This function should only be called on the server
    /// </summary>
    /// <param name="amount">the base damage to apply</param>
    /// <param name="instigator">player state that was responsible for causing this damage (e.g., player who shot the gun)</param>
    /// <param name="causer">gameobject that actually caused this damage (e.g., the grenade that exploded)</param>
    /// <param name="type">damage type</param>
    public void ApplyDamage(int amount, PlayerState instigator, GameObject causer, DamageType type);
}
