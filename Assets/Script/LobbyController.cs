using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;
    public TMP_Text lobbyNameText;

    public GameObject playerListViewContent;
    public GameObject playerListItemPrefab;
    public GameObject LocalPlayerObject;

    public ulong CurrentLobbyID;
    public bool playerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController localPlayerController;

    public Button startGameButton;
    public TMP_Text readyButtonText;


    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }

    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyId;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if (!playerItemCreated)
        {
            CreateHostPlayerItem();
        }
        if (PlayerListItems.Count < manager.GamePlayers.Count)
        {
            CreateClientPlayerItem();
        }
        if (PlayerListItems.Count > manager.GamePlayers.Count)
        {
            RemovePlayerItem();
        }
        if (PlayerListItems.Count == manager.GamePlayers.Count)
        {
            UpdatePlayerItem();
        }
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject NewPlayerItem = Instantiate(playerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.playerName;
            NewPlayerItemScript.ConnectionID = player.connectionID;
            NewPlayerItemScript.playerSteamID = player.playerSteamID;
            NewPlayerItemScript.ready = player.ready;
            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(playerListViewContent.transform);
            NewPlayerItem.transform.localScale = Vector3.one;

            PlayerListItems.Add(NewPlayerItemScript);
        }

        playerItemCreated = true;
    }
    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in manager.GamePlayers)
        {
            if (!PlayerListItems.Any(b => b.ConnectionID == player.connectionID))
            {
                GameObject NewPlayerItem = Instantiate(playerListItemPrefab) as GameObject;
                PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

                NewPlayerItemScript.PlayerName = player.playerName;
                NewPlayerItemScript.ConnectionID = player.connectionID;
                NewPlayerItemScript.playerSteamID = player.playerSteamID;
                NewPlayerItemScript.ready = player.ready;
                NewPlayerItemScript.SetPlayerValues();

                NewPlayerItem.transform.SetParent(playerListViewContent.transform);
                NewPlayerItem.transform.localScale = Vector3.one;

                PlayerListItems.Add(NewPlayerItemScript);
            }
           
        }

    }
    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in manager.GamePlayers)
        {
            foreach (PlayerListItem playerListItemScript in PlayerListItems)
            {
                if (playerListItemScript.ConnectionID == player.connectionID)
                {
                    playerListItemScript.PlayerName = player.playerName;
                    playerListItemScript.ready = player.ready;
                    playerListItemScript.SetPlayerValues();
                    if (player == localPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }

        }
        CheckIsAllready();
    }
    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();
        foreach (var playerListItem in PlayerListItems)
        {
            if (!manager.GamePlayers.Any(b => b.connectionID == playerListItem.ConnectionID))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }
        if (playerListItemsToRemove.Count > 0)
        {
            foreach (PlayerListItem playerListItemToRemove in playerListItemsToRemove)
            {
                GameObject ObjectToRemove = playerListItemToRemove.gameObject;
                PlayerListItems.Remove(playerListItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        localPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();

        
    }

    public void ReadyPlayer()
    {
        localPlayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        if (localPlayerController.ready)
        {
            readyButtonText.text = "Not ready";
        }
        else
        {
            readyButtonText.text = "Ready";
        }
    }
    
    public void CheckIsAllready()
    {
        bool allReady = false;
        foreach (var player in Manager.GamePlayers)
        {
            if (player.ready)
            {
                allReady = true;
            }
            else
            {
                allReady = false;
                break;
            }
        }
        if (allReady)
        {
            if (localPlayerController.playerIDNumber == 1)
            {
                startGameButton.interactable = true;
            }
            else
            {
                startGameButton.interactable = false;
            }
        }
        else
        {
            startGameButton.interactable = false;
        }
    }

    public void StartGame(string sceneName)
    {
        localPlayerController.CanStartGame(sceneName);
    }

    
}
