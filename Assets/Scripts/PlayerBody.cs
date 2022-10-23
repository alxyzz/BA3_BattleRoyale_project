using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    //to be added on the player prefab.
    //float health = 100; 

    public GameObject CameraMountPoint; //to allow switching cameras after respawning without creating new ones - we will reuse the same camera, mounted to an empty game object point on the player entity
    // Start is called before the first frame update


    public Weapon w_melee;
    public Weapon w_short;
    public Weapon w_medium;
    public Weapon w_long;






    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetCamera()
    {
        Transform cameraTransform = Camera.main.gameObject.transform;  //Find main camera which is part of the scene instead of the prefab
        cameraTransform.parent = CameraMountPoint.transform;  //Make the camera a child of the mount point
        cameraTransform.position = CameraMountPoint.transform.position;  //Set position/rotation same as the mount point
        cameraTransform.rotation = CameraMountPoint.transform.rotation;

    }



}
