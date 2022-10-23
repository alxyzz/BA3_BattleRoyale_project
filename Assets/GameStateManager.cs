using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
///singleton class to keep certain stuff in memory for the sake of our game, like score or the global player list. 
public class GameStateManager : MonoBehaviour
{

    List<Tuple<PlayerData, string>> players = new List<Tuple<PlayerData, string>>(); // just to keep a reference of the player data linked to the player ip
    GameObject localPlayer;
    int roundNumber;
    public PlayerData player;
    // Start is called before the first frame update
    void Start()
    {

        player = new PlayerData(); //initializes local player mind
        
        if (NetworkServer.active)
        {

        }
        else
        {
                
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }







    public static GameStateManager Instance;

    private void Awake()
    {
        // First time run
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }
}
