using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalGame : MonoBehaviour
{
    private LocalGame _instance;
    private void Awake()
    {
        _instance = this;
    }

    public static GameObject LocalPlayer { get; set; }
    
}
