using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* When rule-related events in the game happen and need to be tracked and shared with all players,
 * that information is stored and synced through the Game State. This information can include:
 *   How long the game has been running (including running time before the local player joined).
 *   When each individual player joined the game, and the current state of that player.
 *   The list of connected players.
 *   The current Game Mode.
 *   Whether or not the game has begun.
 * The Game State is responsible for enabling the clients to monitor the state of the game.
 * Conceptually, the Game State should manage information that is meant to be known to all connected clients.
 */ 
public class GameState : NetworkBehaviour
{
    private static GameState instance;
    private void Awake()
    {
        instance = this;
        
    }

    [SyncVar][HideInInspector] public bool hasBegun;
    public static bool HasBegun => instance.hasBegun;

    private List<PlayerState> _playerStates = new List<PlayerState>();
    public static List<PlayerState> PlayerStates => instance._playerStates;
    public static void AddPlayer(PlayerState ps)
    {
        if (ps != null && !PlayerStates.Contains(ps))
        {
            PlayerStates.Add(ps);
            UIManager.RefreshStatistics();
        }
    }
    public static void RemovePlayer(PlayerState ps)
    {
        if (ps != null && PlayerStates.Contains(ps))
        {
            PlayerStates.Remove(ps);
            UIManager.RefreshStatistics();
        }
    }
}
