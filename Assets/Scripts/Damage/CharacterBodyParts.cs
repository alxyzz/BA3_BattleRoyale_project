using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPart
{
    HEAD,
    BODY,
    ARM,
    THIGH,
    CALF
}
public class CharacterBodyParts : PenetrableBehaviour
{
    [SerializeField] private BodyPart _part;
    public BodyPart Part => _part;
}
