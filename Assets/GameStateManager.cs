using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
///singleton class to keep certain stuff in memory for the sake of our game, like score or the global player list. this should only operate on the host pc
public class GameStateManager : MonoBehaviour
{

    List<Tuple<PlayerData, string>> players = new List<Tuple<PlayerData, string>>(); // just to keep a reference of the player data linked to the player ip
    GameObject localPlayer;
    int roundNumber;
    // Start is called before the first frame update
    void Start()
    {
        GameObject localPlayer = NetworkClient.localPlayer.gameObject;
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







    private static GameStateManager instance = null;
    private static readonly object padlock = new object();

    GameStateManager()
    {
    }

    public static GameStateManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new GameStateManager();
                }
                return instance;
            }
        }
    }
}
