using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/* Game Mode contains the rules of this game, such as:
 *   The number of players present, as well as the maximum number of players allowed.
 *   How players enter the game, which can include rules for selecting spawn locations and other spawn/respawn behavior.
 *   Whether or not the game can be paused, and how pausing the game is handled.
 * The Game Mode is not replicated to any remote clients that join in a multiplayer game; it exists only on the server,
 * so local clients cannot access the actual instance of Game Mode and check its variables to see what has changed as the game progresses.
 */
public class GameMode : NetworkManager
{
    public override void Awake()
    {
        base.Awake();
        GameObject[] weapons = Resources.LoadAll<GameObject>("Weapons/Overworld");
        spawnPrefabs.AddRange(weapons);
    }

}
