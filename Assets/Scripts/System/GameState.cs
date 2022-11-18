using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public enum GameStage
{
    WAITING, // waiting for other players
    READY, // all players connected. Countdown
    PLAYING, // playing
    OVER // winner
}
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
    public override void OnStartServer()
    {
        Debug.Log("Game state OnStartServer.");
        instance = this;
        SteamLobby.Instance.onLobbyChatUpdate += OnLobbyChatUpdate;
    }

    public override void OnStartClient()
    {
        Debug.Log("Game state OnStartClient. Binding callbak method.");
        instance = this;
        // _playerNetIds
        foreach (var item in playerDic)
        {
            UI_GameHUD.Instance.AddPlayerToStatistics(item.Value);           
        }
        playerDic.Callback += PlayerDic_Callback;
    }

    private void PlayerDic_Callback(SyncIDictionary<ulong, uint>.Operation op, ulong key, uint item)
    {
        Debug.Log($"On client game state _playerNetIds changed: {op}.");
        switch (op)
        {
            case SyncIDictionary<ulong, uint>.Operation.OP_ADD:
                UI_GameHUD.Instance.AddPlayerToStatistics(item);
                UpdateConnectedPlayerNum();
                break;
            case SyncIDictionary<ulong, uint>.Operation.OP_CLEAR:
                break;
            case SyncIDictionary<ulong, uint>.Operation.OP_REMOVE:
                UI_GameHUD.Instance.RemovePlayerFromStatistics(item);
                if (_stage == GameStage.WAITING)
                {
                    UpdateConnectedPlayerNum();
                }
                break;
            case SyncIDictionary<ulong, uint>.Operation.OP_SET:
                break;
            default:
                break;
        }
    }

    [ClientRpc]
    private void RpcUpdateConnectedPlayerNum()
    {
        UpdateConnectedPlayerNum();
    }
    [Client]
    private void UpdateConnectedPlayerNum()
    {
        UI_GameHUD.Instance.UpdateConnectedPlayerNum(playerDic.Count, SteamMatchmaking.GetNumLobbyMembers(SteamLobby.Instance.CurrentLobbyId));
    }

    private static GameState instance;
    public static GameState Instance => instance;
    [SyncVar(hook = nameof(OnGameStageChanged))] private GameStage _stage = GameStage.WAITING;
    public GameStage Stage => instance._stage;
    private void OnGameStageChanged(GameStage oldVal, GameStage newVal)
    {
        switch (newVal)
        {
            case GameStage.WAITING:
                break;
            case GameStage.READY:
                break;
            case GameStage.PLAYING:
                if (null != LocalGame.Instance.onClientGameStarted)
                {
                    LocalGame.Instance.onClientGameStarted.Invoke();
                }
                break;
            case GameStage.OVER:
                LocalGame.Instance.onClientGameEnded?.Invoke();
                break;
            default:
                break;
        }  
    }

    public readonly SyncDictionary<ulong, uint> playerDic = new SyncDictionary<ulong, uint>();
    public int ConnectedPlayerNum => playerDic.Count;
    [Server]
    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        Debug.Log($"On lobby chat update. State : {(EChatMemberStateChange)callback.m_rgfChatMemberStateChange}");
        switch ((EChatMemberStateChange)callback.m_rgfChatMemberStateChange)
        {
            case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                if (_stage == GameStage.WAITING)
                {
                    RemovePlayer(callback.m_ulSteamIDUserChanged);
                }
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                break;
            default:
                break;
        }
    }
    [Server]
    public void AddPlayer(ulong steamIdUlong, uint netId)
    {
        if (!playerDic.ContainsKey(steamIdUlong))
        {
            Debug.Log($"Server Game State add player {netId}");
            playerDic.Add(steamIdUlong, netId);
            switch (_stage)
            {
                case GameStage.WAITING:
                    if (IfGameCanStart())
                    {
                        Debug.Log("All players connected!");
                        GameReady();
                    }
                    break;
                case GameStage.READY:
                    break;
                case GameStage.PLAYING:
                    break;
                case GameStage.OVER:
                    break;
            }
        }
    }
    [Server]
    public void RemovePlayer(ulong steamIdUlong)
    {
        Debug.Log($"Server game state remove player {steamIdUlong}");
        if (playerDic.ContainsKey(steamIdUlong)) playerDic.Remove(steamIdUlong);
        else if (_stage == GameStage.WAITING) RpcUpdateConnectedPlayerNum();
        uint winnerNetId;
        bool isDraw;
        switch (_stage)
        {
            case GameStage.WAITING:
                if (IfGameOver(out winnerNetId, out isDraw))
                {
                    GameOver(winnerNetId, isDraw);
                }
                else if (IfGameCanStart())
                {
                    GameReady();
                }
                break;
            case GameStage.READY:
                if (IfGameOver(out winnerNetId, out isDraw))
                {
                    GameOver(winnerNetId, isDraw);
                }
                break;
            case GameStage.PLAYING:
                if (IfGameOver(out winnerNetId, out isDraw))
                {
                    GameOver(winnerNetId, isDraw);
                }
                break;
            case GameStage.OVER:
                break;
            default:
                break;
        }
    }

    //public bool TryGetPlayerStateAt(int index, out PlayerState ps) // usually called on the client
    //{
    //    ps = null;
    //    if (index < 0 || index >= ConnectedPlayerNum) return false;

    //    if (NetworkClient.spawned.TryGetValue(_playerNetIds[index], out NetworkIdentity identity))
    //    {
    //        return identity.TryGetComponent(out ps);
    //    }
    //    return false;
    //}
    public bool TryGetPlayerStateBySteamId(ulong steamIdUlong, out PlayerState ps)
    {
        ps = null;
        if (playerDic.ContainsKey(steamIdUlong))
        {
            if (NetworkClient.spawned.TryGetValue(playerDic[steamIdUlong], out NetworkIdentity identity))
            {
                return identity.TryGetComponent(out ps);
            }
        }        
        return false;
    }
    public bool TryGetPlayerStateByNetId(uint netId, out PlayerState ps)
    {
        ps = null;
        if (playerDic.Values.Contains(netId))
        {
            if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity))
            {
                return identity.TryGetComponent(out ps);
            }
        }
        return false;
    }
    public List<PlayerState> GetPlayerStateList() // usually called on the client
    {
        List<PlayerState> results = new List<PlayerState>();
        foreach (var item in playerDic.Values)
        {
            if (NetworkClient.spawned.TryGetValue(item, out NetworkIdentity identity))
            {
                if (identity.TryGetComponent(out PlayerState ps))
                {
                    results.Add(ps);
                }                
            }
        }
        return results;
    }

    Coroutine _cReadyCountdown;
    private IEnumerator ReadyCountdown()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("3");
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("2");
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("1");
        yield return new WaitForSecondsRealtime(1.0f);
        RpcCountdown("");
        _stage = GameStage.PLAYING;
        LocalGame.Instance.onServerGameStarted?.Invoke();
    }
    [ClientRpc]
    private void RpcCountdown(string str)
    {
        UI_GameHUD.SetCountdown(str);
    }
    [Server]
    public void PlayerDied(uint netId) // only called on the server
    {
        if (IfGameOver(out uint winnerNetId, out bool isDraw))
        {
            GameOver(winnerNetId, isDraw);
        }
    }

    #region End Conditions
    private bool IfGameCanStart()
    {
        return playerDic.Count >= SteamMatchmaking.GetNumLobbyMembers(SteamLobby.Instance.CurrentLobbyId);
    }
    private bool IfGameOver(out uint winnerNetId, out bool isDraw)
    {
        winnerNetId = 0;
        isDraw = false;
        if (SteamMatchmaking.GetNumLobbyMembers(SteamLobby.Instance.CurrentLobbyId) == 1)
        {
            winnerNetId = playerDic.First().Value;
            return true;
        }
        List<PlayerState> livings = GetPlayerStateList().FindAll(x => x.IsAlive);

        switch (livings.Count)
        {
            case 0:
                isDraw = true;
                return true;
            case 1:
                winnerNetId = livings[0].netId;
                isDraw = false;
                return true;
            default:
                return false;
        }
    }
    [Server]
    private void GameReady()
    {
        if (_stage == GameStage.WAITING)
        {
            _stage = GameStage.READY;
            _cReadyCountdown = StartCoroutine(ReadyCountdown());
        }
    }
    [Server]
    private void GameOver(uint winnerNetId, bool isDraw = false)
    {
        if (null != _cReadyCountdown) StopCoroutine(_cReadyCountdown);
        _stage = GameStage.OVER;
        LocalGame.Instance.onServerGameEnded?.Invoke();
        if (isDraw)
        {
            Debug.Log("And then there were none.");
            RpcDecalreWinner(winnerNetId);
        }
        else
        {
            Debug.Log($"Game Over! The winner's net ID is {winnerNetId}.");
            RpcDecalreWinner(winnerNetId);
        }
    }
    [ClientRpc]
    private void RpcDecalreWinner(uint netId)
    {
        if (TryGetPlayerStateByNetId(netId, out PlayerState ps))
        {
            UI_GameHUD.ShowWinner(ps);
        }
    }
    #endregion
}
