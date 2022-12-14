using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using Mirror.Examples.Pong;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _tmpLobbyName;
    [SerializeField] private TextMeshProUGUI _tmpLobbyId;
    [SerializeField] private RectTransform _pnlPlayerList;
    [SerializeField] private Button _btnStartReady;

    private CSteamID _lobbyId;
    private bool _isOwner;
    private Dictionary<CSteamID, UI_Lobby_PlayerItem> _players = new Dictionary<CSteamID, UI_Lobby_PlayerItem>();
    private bool _isReady = false;

    private GameObject _playerItem;

    private void Start()
    {
        if (null == SteamLobby.Instance)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }
        _playerItem = Resources.Load<GameObject>("UI/Lobby/LobbyPlayerItem");

        _lobbyId = SteamLobby.Instance.CurrentLobbyId;
        _isOwner = SteamMatchmaking.GetLobbyOwner(_lobbyId) == SteamUser.GetSteamID();
        _isReady = _isOwner;
        if (_isOwner)
        {
            SteamMatchmaking.SetLobbyData(_lobbyId, SteamLobby.keyGameStarted, "0");
            SteamMatchmaking.SetLobbyJoinable(_lobbyId, true);
        }

        SteamMatchmaking.SetLobbyMemberData(_lobbyId, SteamLobby.keyReady, _isReady ? "1" : "0");

        UpdateLobbyName();
        InitLobbyId();
        InitPlayerList();
        InitStartReadyButton();

        //// UpdateButton();
        SteamLobby.Instance.onLobbyChatUpdate += OnLobbyChatUpdate;
        SteamLobby.Instance.onLobbyDataUpdate += OnLobbyDataUpdate;

    }

    private void OnDisable()
    {
        Debug.Log("on lobby controller disable");
        if (SteamLobby.Instance)
        {
            SteamLobby.Instance.onLobbyChatUpdate -= OnLobbyChatUpdate;
            SteamLobby.Instance.onLobbyDataUpdate -= OnLobbyDataUpdate;
        }
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        Debug.Log("On lobby data update");

        if (callback.m_ulSteamIDLobby == callback.m_ulSteamIDMember) // Lobby data changed
        {
            UpdateLobbyName();
        }
        else // user data changed
        {
            CSteamID playerId = new CSteamID(callback.m_ulSteamIDMember);
            _players[playerId].Refresh();
        }
    }
    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        Debug.Log("On lobby chat update.");
        switch ((EChatMemberStateChange)callback.m_rgfChatMemberStateChange)
        {
            case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                AddPlayerListItem(new CSteamID(callback.m_ulSteamIDUserChanged));
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                RemovePlayerListItem(new CSteamID(callback.m_ulSteamIDUserChanged));
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                RemovePlayerListItem(new CSteamID(callback.m_ulSteamIDUserChanged));
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                RemovePlayerListItem(new CSteamID(callback.m_ulSteamIDUserChanged));
                break;
            case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                RemovePlayerListItem(new CSteamID(callback.m_ulSteamIDUserChanged));
                break;
            default:
                break;
        }
    }

    public void UpdateLobbyName()
    {
        string lobby;
        if ((lobby = SteamMatchmaking.GetLobbyData(_lobbyId, SteamLobby.keyLobbyName)) != "")
        {
            _tmpLobbyName.SetText(lobby);
        }
        else
        {
            _tmpLobbyName.SetText($"<#FFF200>{SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyOwner(_lobbyId))}</color>'s Lobby");
        }
    }
    private void InitLobbyId()
    {
        _tmpLobbyId.SetText($"Lobby ID : {_lobbyId}");
    }
    public void CopyLobbyId()
    {
        TextEditor t = new TextEditor();
        t.text = _lobbyId.ToString();
        t.SelectAll();
        t.Copy();
    }
    private void InitPlayerList()
    {
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(_lobbyId); i++)
        {
            AddPlayerListItem(SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i));
        }
    }

    private void AddPlayerListItem(CSteamID playerId)
    {
        UI_Lobby_PlayerItem item = Instantiate(_playerItem, _pnlPlayerList).GetComponent<UI_Lobby_PlayerItem>();
        _players.Add(playerId, item);
        item.Initialise(playerId);
    }
    private void RemovePlayerListItem(CSteamID playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            if (null != _players[playerId].gameObject)
            {
                Destroy(_players[playerId].gameObject);
            }
            _players.Remove(playerId);            
        }        
    }
    
    private bool CanStartGame()
    {
        // if (_players.Count < 2) return false;
        foreach (var item in _players)
        {
            string ready = SteamMatchmaking.GetLobbyMemberData(
                _lobbyId,
                item.Key,
                SteamLobby.keyReady
                );
            if (ready != "1")
            {
                MasterUIManager.AddPopupHint("There are players unready...");
                return false;
            }
        }
        
        return true;
    }

    private void InitStartReadyButton()
    {
        _btnStartReady.GetComponentInChildren<TextMeshProUGUI>().SetText(_isOwner ? "Start" : "Ready");
        // _btnStartReady.interactable = !_isOwner;
    }

    public void StartOrReady()
    {
        if (SteamMatchmaking.GetLobbyData(_lobbyId, SteamLobby.keyGameStarted) != "0")
        {
            MasterUIManager.AddPopupHint("The game has already begun.");
            return;
        }

        if (_isOwner)
        {
            if (CanStartGame())
            {
                Debug.Log("Can start game!");
                SteamMatchmaking.SetLobbyJoinable(_lobbyId, false);
                SteamMatchmaking.SetLobbyData(_lobbyId, SteamLobby.keyGameStarted, "1");

                // SteamLobby.SceneToLoad = "MainMap";
                MyNetworkManager.singleton.StartGame();
            }
        }
        else
        {
            _isReady = !_isReady;
            SteamMatchmaking.SetLobbyMemberData(
                _lobbyId,
                SteamLobby.keyReady,
                _isReady ? "1" : "0");
            _btnStartReady.GetComponentInChildren<TextMeshProUGUI>().SetText(_isReady ? "Unready" : "Ready");
        }
    }
}
