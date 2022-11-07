using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenetrableBehaviour : MonoBehaviour, IPenetrable
{
    [SerializeField] protected PenetrableMaterial _material;
    [SerializeField]
    [Range(0.0f,1.0f)]
    protected float _thicknessMultiplier = 1.0f;

    public float GetAttenuation(float multiplier, WeaponData data)
    {
        float result = multiplier;
        result *= data.GetPenetrationAttenuation(_material);
        result *= _thicknessMultiplier;
        return result;
    }
}
