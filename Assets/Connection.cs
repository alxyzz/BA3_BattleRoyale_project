using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Connection : MonoBehaviour
{
    public NetworkManager NetworkManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isBatchMode)
        {
            NetworkManager.StartClient();
        }
    }

    public void JoinClient()
    {
        NetworkManager.networkAddress = "46.138.92.108";
        NetworkManager.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
