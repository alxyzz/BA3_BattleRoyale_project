using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Chat_MsgBox : UI_Widget
{
    [SerializeField] private RectTransform _msgBack;
    [SerializeField] private TMP_InputField _ifMsg;
    [SerializeField] private GameObject _pfbMsgItem;
    private bool _isOpened;
    private void Start()
    {
        Close(0);
        Callback<LobbyChatMsg_t>.Create(OnRecievedLobbyChatMsg);
        // SteamMatchmaking.SendLobbyChatMsg
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_isOpened)
            {
                SendChatMsg();
                Close();
            }
            else
            {
                Open();
            }
        }
        ;        
    }
    public void Open(float duration = 0.2f)
    {
        _isOpened = true;
        Fade(1, duration, () => { _ifMsg.Select(); });
        Translate(Vector3.left * 30, duration);
    }
    public void Close(float duration = 0.15f)
    {
        _isOpened = false;
        Fade(0, duration);
        Translate(Vector3.right * 30, duration);
    }
    private void OnRecievedLobbyChatMsg(LobbyChatMsg_t callback)
    {
        byte[] pvData = new byte[4000];
        int len = SteamMatchmaking.GetLobbyChatEntry(
            new CSteamID(callback.m_ulSteamIDLobby),
            (int)callback.m_iChatID,
            out CSteamID userId,
            pvData,
            4000,
            out _
            );
        UI_Chat_MsgItem item = Instantiate(_pfbMsgItem, _msgBack).GetComponent<UI_Chat_MsgItem>();
        // item.transform.SetParent(_msgBack, false);
        item.StartCoroutine(item.SetChatContent(SteamFriends.GetFriendPersonaName(userId),
            Encoding.UTF8.GetString(pvData, 0, len),
            userId == SteamUser.GetSteamID()));
    }

    private void SendChatMsg()
    {
        string str = _ifMsg.text.TrimEnd();
        if (str != string.Empty)
        {
            byte[] msg = Encoding.UTF8.GetBytes(str);
            _ifMsg.SetTextWithoutNotify("");
            SteamMatchmaking.SendLobbyChatMsg(SteamLobby.Instance.CurrentLobbyId, msg, msg.Length);
        }
    }
}
