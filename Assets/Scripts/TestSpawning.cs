using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawning : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            CmdTest();
        }
    }

    [Command]
    public void CmdTest()
    {
        RpcTest();
    }
    [ClientRpc]
    public void RpcTest()
    {
        Debug.Log("P click!");
    }
}
