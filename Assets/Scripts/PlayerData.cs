using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    string ipAddresss;
    PlayerBody currentBody;

    int kills;
    int deaths;
    int assists;

    public enum living_state
    {
        Dead,
        Alive,
        Observing
    }

    public living_state state = living_state.Observing;
    /// <summary>
    /// check periodically for leavers then run this so the player's character dies and drops their stuff
    /// </summary>
    public void Disconnect()
    {

    }

    public void Respawn()
    {

        //init new body at random location if round is not in progress
        //reset camera
    }


}
