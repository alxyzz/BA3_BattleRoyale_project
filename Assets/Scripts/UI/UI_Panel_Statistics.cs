using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class UI_Panel_Statistics : UI_Widget
{
    [Header("Components")]
    [SerializeField] private RectTransform _list;
    [SerializeField] private TextMeshProUGUI _tmpFps;
    [SerializeField] private TextMeshProUGUI _tmpLobbyName;

    [Header("Resources")]
    [SerializeField] private GameObject _pfbPlayerSlot;
    private List<UI_Stat_PlayerSlot> _slots = new List<UI_Stat_PlayerSlot>();
    
    // Start is called before the first frame update
    void Start()
    {
        RenderOpacity = 0.0f;
        StartCoroutine(UpdateFps());

        string lobby;
        if ((lobby = SteamMatchmaking.GetLobbyData(SteamLobby.Instance.CurrentLobbyId, SteamLobby.keyLobbyName)) != "")
        {
            _tmpLobbyName.SetText(lobby);
        }
        else
        {
            _tmpLobbyName.SetText($"{SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyOwner(SteamLobby.Instance.CurrentLobbyId))}'s Lobby");
        }
    }

    IEnumerator UpdateFps()
    {
        int frame = 0;
        float delta = 0.0f;
        while (true)
        {
            if (delta > 1)
            {
                _tmpFps.SetText((frame / delta).ToString("#0.0") + " FPS");
                frame = 0;
                delta = 0.0f;
            }
            else
            {
                frame++;
                delta += Time.deltaTime;
            }
            yield return null;
        }
    }

    public void SetShown(bool shown)
    {
        if (shown)
        { 
            Fade(1, 0.2f);
        }
        else
        {
            Fade(0, 0.1f);
        }
    }

    public void AddPlayerSlot(PlayerState ps)
    {
        UI_Stat_PlayerSlot slot = Instantiate(_pfbPlayerSlot, _list).GetComponent<UI_Stat_PlayerSlot>();
        slot.Initialise(ps);
        _slots.Add(slot);

    }
    public void RemovePlayerSlot(PlayerState ps)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null && _slots[i].Player == ps)
            {
                GameObject obj = _slots[i].gameObject;
                _slots.RemoveAt(i);
                if(obj != null) Destroy(obj);
                return;
            }            
        }
    }

    //public void Reorder()
    //{
    //    _list.GetChild(0).si
    //}
    //public void Refresh()
    //{
    //    foreach (var item in _slots)
    //    {
    //        Destroy(item.gameObject);
    //    }
    //    _slots.Clear();
    //    foreach (var item in GameState.PlayerStates)
    //    {
    //        UI_Stat_PlayerSlot slot = Instantiate(_pfbPlayerSlot, _list).GetComponent<UI_Stat_PlayerSlot>();
    //        slot.Initialise(item);
    //        _slots.Add(slot);

    //    }
    //}
}