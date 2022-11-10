using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PenetrableMaterial
{
    WOOD,
    STONE,
    METAL,
    GLASS,
    WATER,
    HUMAN
}
public interface IPenetrable
{
    public float GetAttenuation(float multiplier, WeaponData data);
}
