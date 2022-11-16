using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public static GameState Instance => instance;
    private void Awake()
    {
        if (null == SteamLobby.Instance)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        instance = this;
        NetworkClient.AddPlayer();
        //Debug.Log("Game state Awake + " + SceneManager.GetActiveScene().name);
        //if (isServer)
        //    MyNetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        //NetworkClient.AddPlayer();
    }

    public Action onGameStarted; // only called on the server
    public void StartGame() // only called on the server
    {
        hasBegun = true;
        onGameStarted?.Invoke();
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

            if (instance.isServer)
            {
                if (SteamMatchmaking.GetNumLobbyMembers(SteamLobby.Instance.CurrentLobbyId) == PlayerStates.Count)
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
    
    private IEnumerator CountdownStart() // only called by server
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

    public static void PlayerDie(PlayerState ps) // only called on the server
    {
        if (instance._playerStates.Contains(ps))
        {
            List<PlayerState> livings = instance._playerStates.FindAll(x => x.IsAlive);
            switch (livings.Count)
            {
                case 0:
                    instance.DeclareTie();
                    break;
                case 1:
                    instance.DeclareVictory(livings[0]);
                    break;
                default:
                    break;
            }
        }
    }

    #region End Conditions
    private void DeclareVictory(PlayerState winner)
    {
        Debug.Log($"player {SteamFriends.GetFriendPersonaName(winner.SteamId)} win!");
        hasBegun = false;
        UI_GameHUD.ShowWinner(winner);
    }

    private void DeclareTie()
    {
        Debug.Log("Game Draw!");
        hasBegun = false;
        UI_GameHUD.ShowWinner(_playerStates[0]);
    }
    //public void UpdateState(ISubject subject)
    //{
    //    List<PlayerState> _livingPlayers = _playerStates.FindAll(x => x.IsAlive);
    //    switch (_livingPlayers.Count)
    //    {
    //        case 1:
    //            DeclareVictory(_livingPlayers[0]);
    //            break;
    //        case 0:
    //            DeclareTie();
    //            break;
    //        default:
    //            break;
    //    }
    //}
    #endregion
}
