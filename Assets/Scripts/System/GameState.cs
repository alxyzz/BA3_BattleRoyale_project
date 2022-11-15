using Mirror;
using Steamworks;
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
public class GameState : NetworkBehaviour, IObserver
{

    public static GameState instance;
    private void Awake()
    {
        instance = this;
        
    }
    
    public void StartGame()
    {
        hasBegun = true;

        foreach (var item in _playerStates)
        {
            item.TargetInitialWeapon();
        }        
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
            UI_GameHUD.AddPlayerToStatistics(ps);

            if (SteamMatchmaking.GetNumLobbyMembers(SteamLobby.Instance.CurrentLobbyId) == PlayerStates.Count)
            {
                // all members arrived
                if (instance.isServer)
                {
                    instance.StartCoroutine(instance.CountdownStart());
                }
            }            
        }
    }
    public static void RemovePlayer(PlayerState ps)
    {
        if (ps != null && PlayerStates.Contains(ps))
        {
            PlayerStates.Remove(ps);
            UI_GameHUD.RemovePlayerFromStatistics(ps);
        }
    }

    private IEnumerator CountdownStart()
    {
        RpcCountdown("3");
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("2");
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("1");
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("");
        StartGame();
    }
    [ClientRpc]
    private void RpcCountdown(string str)
    {
        UI_GameHUD.SetCountdown(str);
    }


    #region End Conditions
    private void DeclareVictory(PlayerState winner)
    {

    }

    private void DeclareTie()
    {

    }

    public void UpdateState(ISubject subject)
    {
        List<PlayerState> _livingPlayers = _playerStates.FindAll(x => x.IsAlive);
        switch (_livingPlayers.Count)
        {
            case 1:
                DeclareVictory(_livingPlayers[0]);
                break;
            case 0:
                DeclareTie();
                break;
            default:
                break;
        }
    }
    #endregion
}
