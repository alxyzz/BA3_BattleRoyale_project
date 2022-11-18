using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalGame : MonoBehaviour
{
    private static LocalGame instance;
    public static LocalGame Instance => instance;
    private void Awake()
    {
        instance = this;
    }
    

    public Action onServerGameStarted;
    public Action onClientGameStarted;
    public Action onServerGameEnded;
    public Action onClientGameEnded;
}
