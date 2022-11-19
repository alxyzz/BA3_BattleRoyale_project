using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat_PlayerSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmpNickname;
    [SerializeField] private TextMeshProUGUI _tmpKills;
    [SerializeField] private TextMeshProUGUI _tmpPing;
    private RawImage _imgBack;
    public uint NetId { get; private set; }
    private PlayerState _playerState;

    private void Awake()
    {
        _imgBack = GetComponent<RawImage>();
    }
    public void Initialise(uint netId)
    {
        NetId = netId;
        if (GameState.Instance.TryGetPlayerStateByNetId(netId, out _playerState))
        {
            Debug.Log($"player slot netID {netId}");
            Debug.Log($"{_playerState.Nickname}");
            _tmpNickname.SetText(_playerState.Nickname);
            _imgBack.color = _playerState.isLocalPlayer ? new Color(0.5f, 0.6f, 0.7f, 0.235f) : Color.clear;
            _tmpPing.SetText(_playerState.Ping.ToString());
            _tmpKills.SetText(_playerState.Kills.ToString());

            _playerState.onNicknameChanged += OnPlayerNicknameChanged;
            _playerState.onPingChanged += OnPlayerPingChanged;
            _playerState.onKillsChanged += OnPlayerKillsChanged;
            _playerState.onHealthChanged += OnPlayerHealthChanged;
        }
    }

    private void OnPlayerNicknameChanged(string str)
    {
        _tmpNickname.SetText(str);
    }
    private void OnPlayerKillsChanged(int val)
    {
        _tmpKills.SetText(val.ToString());

        for (int i = transform.GetSiblingIndex(); i > 0; i--)
        {
            Debug.Log($"{i}  {transform.parent.GetChild(i - 1).GetComponent<UI_Stat_PlayerSlot>()._playerState == null}");
            if (transform.parent.GetChild(i - 1).GetComponent<UI_Stat_PlayerSlot>()._playerState.Kills >= val)
            {
                transform.SetSiblingIndex(i);
                return;
            }
        }
        transform.SetSiblingIndex(0);
    }
    private void OnPlayerPingChanged(int val)
    {
        _tmpPing.SetText(val.ToString());
    }

    private void OnPlayerHealthChanged(int val)
    {
        if (val <= 0)
        {
            _tmpNickname.color = Color.gray;
            _tmpKills.color = Color.gray;
            _tmpPing.color = Color.gray;
        }
    }
}

