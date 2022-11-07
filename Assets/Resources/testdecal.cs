using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testdecal : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;

        Invoke(nameof(DestroySelf), 8f);
    }

    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
