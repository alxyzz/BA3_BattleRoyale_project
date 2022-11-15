using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks;

public class UI_Lobby_PlayerItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private RawImage _imgIcon;
    [SerializeField] private TextMeshProUGUI _tmpName;
    [SerializeField] private Image _imgReady;

    [Header("Owner or Ready")]
    [SerializeField] private Sprite _spriteOwner;
    [SerializeField] private Color _colourOwner;
    [SerializeField] private Sprite _spriteReady;
    [SerializeField] private Color _colourReady;
    private CSteamID _playerId;
    private bool _isOwner;
    // public string PlayerName;
    //private int _connectionId;
    //private ulong _playerSteamId;

    //private bool _avatarRecieved;

    //public bool ready;

    //protected Callback<AvatarImageLoaded_t> ImageLoaded;

    private void Start()
    {
        Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    public void Initialise(CSteamID playerId)
    {
        _playerId = playerId;
        _isOwner = SteamMatchmaking.GetLobbyOwner(SteamLobby.Instance.CurrentLobbyId) == playerId;

        SetPlayerIcon();
        SetPlayerName();
        SetOwnerReadyIcon();
    }

    private void SetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar(_playerId);
        if (ImageID == -1)
        {
            return;
        }
        _imgIcon.texture = SteamLobby.GetSteamImageAsTexture(ImageID);
    }
    

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID == _playerId)
        {
            _imgIcon.texture = SteamLobby.GetSteamImageAsTexture(callback.m_iImage);
        }
    }

    private void SetPlayerName()
    {
        _tmpName.SetText(SteamFriends.GetFriendPersonaName(_playerId));
    }
    private void SetOwnerReadyIcon()
    {
        _imgReady.sprite = _isOwner ? _spriteOwner : _spriteReady;
        _imgReady.color = _isOwner ? _colourOwner : _colourReady;
        //_imgReady.CrossFadeAlpha(0, 0, true);
        //return;
        string ready = SteamMatchmaking.GetLobbyMemberData(
            SteamLobby.Instance.CurrentLobbyId,
            _playerId,
            SteamLobby.keyReady);
        _imgReady.CrossFadeAlpha(
            ready == "1" ? 1 : 0,
            0f,
            true
            );
    }
    private void RefreshReadyIcon()
    {
        string ready = SteamMatchmaking.GetLobbyMemberData(
            SteamLobby.Instance.CurrentLobbyId,
            _playerId,
            SteamLobby.keyReady);
        _imgReady.CrossFadeAlpha(
            ready == "1" ? 1 : 0,
            0.15f,
            true
            );
    }

    public void Refresh()
    {
        _isOwner = SteamMatchmaking.GetLobbyOwner(SteamLobby.Instance.CurrentLobbyId) == _playerId;
        _imgReady.sprite = _isOwner ? _spriteOwner : _spriteReady;
        _imgReady.color = _isOwner ? _colourOwner : _colourReady;
        if (_isOwner)
            _imgReady.CrossFadeAlpha(1, 0, true);
        else
            RefreshReadyIcon();
    }
}
