using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat_PlayerSlot : MonoBehaviour
{
    [SerializeField] private Text _txtNickname;
    [SerializeField] private Text _txtKills;
    [SerializeField] private Text _txtPing;
    private RawImage _imgBack;
    public PlayerState Player { get; private set; }

    private void Awake()
    {
        _imgBack = GetComponent<RawImage>();
    }
    public void Initialise(PlayerState ps)
    {
        Player = ps;
        _txtKills.text = ps.kills.ToString();
        _txtPing.text = ps.ping.ToString();
        ps.onPingChanged += OnPlayerStatChanged;
        ps.onKillsChanged += OnPlayerStatChanged;

        _imgBack.color = ps.isLocalPlayer ? new Color(0.5f, 0.6f, 0.7f, 0.235f) : Color.clear;
    }

    private void OnPlayerStatChanged()
    {
        _txtKills.text = Player.kills.ToString();
        _txtPing.text = Player.ping.ToString();

    }
}

