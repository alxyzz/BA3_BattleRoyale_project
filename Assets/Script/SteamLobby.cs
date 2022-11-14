using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;
using System;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance { get; private set; }

    // Callbacks
    public Action RecoverUI;
    //protected Callback<LobbyCreated_t> LobbyCreated;
    //protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    //protected Callback<LobbyEnter_t> LobbyEntered;

    public CSteamID CurrentLobbyId { get; private set; }

    // Variables
    public static readonly string keyHostAddress = "HostAddress";
    public static readonly string keyLobbyName = "LobbyName";

    public static readonly string keyReady = "Ready";

    private MyNetworkManager _manager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _manager = GetComponent<MyNetworkManager>();

    }
    void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _manager.maxConnections);
    }
    public void JoinLobby(ulong id)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(id));
    }
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        Debug.Log("On Lobby created.");
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(
            lobbyId, keyHostAddress,
            SteamUser.GetSteamID().ToString());
        //SteamMatchmaking.SetLobbyData(
        //    lobbyId, keyLobbyName, 
        //    $"<#FFF210>{SteamFriends.GetPersonaName()}</color>'s lobby");
    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log($"On Lobby entered. Response : {(EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse}");

        switch ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse)
        {
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:
                CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                
                if (SteamMatchmaking.GetLobbyOwner(CurrentLobbyId) == SteamUser.GetSteamID())
                {
                    _manager.StartHost();
                }
                else
                {
                    _manager.networkAddress = SteamMatchmaking.GetLobbyData(CurrentLobbyId, keyHostAddress);
                    _manager.StartClient();
                }
                return;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist:
                MasterUIManager.AddPopupHint("Lobby does not exist...");
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseFull:
                MasterUIManager.AddPopupHint("The lobby is full...");
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseError:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseClanDisabled:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember:
                break;
            case EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded:
                break;
            default:
                break;
        }
        RecoverUI?.Invoke();
    }
}
