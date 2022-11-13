using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalZoneBehaviour : MonoBehaviour
{
    public float radius = 200f;
    public GameObject zoneInner;
    public GameObject zoneOuter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        zoneInner.transform.localScale = new Vector3(radius * 2, 1, radius * 2);
        zoneOuter.transform.localScale = new Vector3(radius * 2, 1, radius * 2);
    }
}
