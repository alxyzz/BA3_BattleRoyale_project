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
    public PlayerState Player { get; private set; }

    private void Awake()
    {
        _imgBack = GetComponent<RawImage>();
    }
    public void Initialise(PlayerState ps)
    {
        Player = ps;
        ps.onNicknameChanged += OnPlayerNicknameChanged;
        ps.onPingChanged += OnPlayerPingChanged;
        ps.onKillsChanged += OnPlayerKillsChanged;
        Player.onDied += OnPlayerDied;

        _imgBack.color = ps.isLocalPlayer ? new Color(0.5f, 0.6f, 0.7f, 0.235f) : Color.clear;
        _tmpPing.SetText(Player.ping.ToString());
        _tmpKills.SetText(Player.kills.ToString());
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
            if (transform.parent.GetChild(i - 1).GetComponent<UI_Stat_PlayerSlot>().Player.kills >= val)
            {
                transform.SetSiblingIndex(i);
            }
        }
    }
    private void OnPlayerPingChanged(int val)
    {
        _tmpPing.SetText(val.ToString());
    }

    private void OnPlayerDied()
    {
        _tmpNickname.color = Color.gray;
        _tmpKills.color = Color.gray;
        _tmpPing.color = Color.gray;
    }
}

