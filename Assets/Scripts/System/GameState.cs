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
        instance = this;
    }

    private void Start()
    {
        if (isClient) NetworkClient.AddPlayer();
    }

    public Action onGameStarted;
    public Action onGameEnded;
    [Command(requiresAuthority = false)]
    public void CmdGameStart()
    {
        _hasBegun = true;
        RpcGameStart();
    }
    [ClientRpc]
    private void RpcGameStart()
    {
        _hasBegun = true;
        onGameStarted?.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdGameEnd()
    {
        _hasBegun = false;
    }
    [ClientRpc]
    private void RpcGameEnd()
    {
        _hasBegun = false;
        onGameEnded?.Invoke();
    }

    private bool _hasBegun;
    public static bool HasBegun => instance._hasBegun;
    private List<PlayerState> _playerStates = new List<PlayerState>();
    public List<PlayerState> PlayerStates => _playerStates;
    public void AddPlayer(PlayerState ps)
    {
        if (ps != null && !_playerStates.Contains(ps))
        {
            _playerStates.Add(ps);
            int numTotalPlayers = SteamMatchmaking.GetNumLobbyMembers(SteamLobby.Instance.CurrentLobbyId);
            UI_GameHUD.AddPlayerToStatistics(ps);
            UI_GameHUD.RefreshJoinedPlayerNum(_playerStates.Count, numTotalPlayers);

            if (instance.isServer)
            {
                if (numTotalPlayers == _playerStates.Count)
                {
                    instance.StartCoroutine(instance.CountdownStart());
                }
            }
        }
    }
    public void RemovePlayer(PlayerState ps)
    {
        if (ps != null && _playerStates.Contains(ps))
        {
            _playerStates.Remove(ps);
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
        CmdGameStart();
    }
    [ClientRpc]
    private void RpcCountdown(string str)
    {
        UI_GameHUD.SetCountdown(str);
    }

    public static void PlayerDie(PlayerState ps) // only called on the server
    {
        if (!HasBegun) return;
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
    private void DeclareVictory(PlayerState winner) // only called by the server
    {
        Debug.Log($"player {SteamFriends.GetFriendPersonaName(winner.SteamId)} win!");
        CmdGameEnd();
        RpcDecalreWinner(_playerStates.IndexOf(winner));
    }
    [ClientRpc]
    private void RpcDecalreWinner(int index)
    {
        UI_GameHUD.ShowWinner(_playerStates[index]);
    }
    private void DeclareTie()
    {
        Debug.Log("Game Draw!");
        CmdGameEnd();
        RpcDecalreWinner(0);
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
