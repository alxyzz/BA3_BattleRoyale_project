using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _ifLobbyId;
    private void Start()
    {
        SteamLobby.Instance.RecoverUI += () => { SetChildrenEnabled(true); };
    }
    private void SetChildrenEnabled(bool enabled)
    {
        foreach (var item in GetComponentsInChildren<Selectable>())
        {
            item.interactable = enabled;
        }       
    }
    public void OnClickHost()
    {
        SetChildrenEnabled(false);
        SteamLobby.Instance.HostLobby();
    }

    public void OnClickJoin()
    {
        SetChildrenEnabled(false);
        if (ulong.TryParse(_ifLobbyId.text, out ulong result))
        {
            SteamLobby.Instance.JoinLobby(result);
        }
        else
        {
            MasterUIManager.AddPopupHint("The LobbyID is not a number...");
            SetChildrenEnabled(true);
        }
    }
}
