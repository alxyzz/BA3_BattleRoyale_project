using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawning : NetworkBehaviour
{
    public GameObject spawning;
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            LevelManager.Instance.CmdSpawnWeaponOverworld("AK47");
        }
    }
}
