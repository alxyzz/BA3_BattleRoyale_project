using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using System.Security.Cryptography;
using System.Text;
using TMPro;

[System.Serializable]
public class Match : NetworkBehaviour
{
    public string ID;
    public List<GameObject> players = new List<GameObject>();

    public Match(string ID, GameObject player)
    {
        this.ID = ID;
        players.Add(player);
    }
}

public class MainMenu : NetworkBehaviour
{
    public static MainMenu instance;
    public readonly SyncList<Match> matches = new SyncList<Match>();
    public readonly SyncList<string> matchIDs = new SyncList<string>();
    private NetworkManager networkManager;

    [Header("MainMenu")]
    public TMP_InputField joinInput;
    public Button hostButton;
    public Button joinButton;
    public Button changeNameButton;
    public Canvas lobbyCanvas;

    [Header("Name")]
    public GameObject changeNamePanel;
    public GameObject closeButton;
    public Button SetButtonName;
    public TMP_InputField nameInput;
    public int firstTime = 1;
    [SyncVar] public string DisplayName;

    [Header("Lobby")]
    public Transform UIPlayerParent;
    public GameObject UIPlayerPrefab;
    public TMP_Text IDText;
    public Button beginGameButton;
    public GameObject TurnManager;
    public bool inGame;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        networkManager = FindObjectOfType<NetworkManager>();
        firstTime = PlayerPrefs.GetInt("firstTime", 1);

        if (!PlayerPrefs.HasKey("Name"))
        {
            return;
        }
        string defaultName = PlayerPrefs.GetString("Name");
        nameInput.text = defaultName;
        DisplayName = defaultName;
        SetName(defaultName);
    }

    public void SetName(string name)
    {
        SetButtonName.interactable = !string.IsNullOrEmpty(name);
    }

    // Update is called once per frame
    void Update()
    {
        if (!inGame)
        {
            Player[] players = FindObjectsOfType<Player>();
            for (int i = 0; i < players.Length; i++)
            {
                players[i].gameObject.transform.localScale = Vector3.zero;
            }

            if (firstTime == 1)
            {
                changeNamePanel.SetActive(true);
                closeButton.SetActive(false);
            }
            else 
            {
                //changeNamePanel.SetActive(false);
            }
            PlayerPrefs.SetInt("firstTime", firstTime);
        }
    }

    public void SaveName()
    {
        joinInput.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        changeNameButton.interactable = false;
        firstTime = 0;
        changeNamePanel.SetActive(false);
        DisplayName = nameInput.text;
        PlayerPrefs.SetString("Name", DisplayName);
        Invoke(nameof(Disconnect), 1f);
    }

    void Disconnect()
    {
        if (networkManager.mode == NetworkManagerMode.Host)
        {
            networkManager.StopHost();
        }
        else if (networkManager.mode == NetworkManagerMode.ClientOnly)
        {
            networkManager.StopClient();
        }
    }

    public void Host()
    {
        joinInput.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        changeNameButton.interactable = false;

        Player.localPlayer.HostGame();
    }

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;
            beginGameButton.interactable = true;
        }
        else
        {
            joinInput.interactable = true;
            hostButton.interactable = true;
            joinButton.interactable = true;
            changeNameButton.interactable = false;
        }
    }

    public void Join()
    {
        joinInput.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        changeNameButton.interactable = false;

        Player.localPlayer.JoinGame(joinInput.text.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;
            beginGameButton.interactable = false;
        }
        else
        {
            joinInput.interactable = true;
            hostButton.interactable = true;
            joinButton.interactable = true;
            changeNameButton.interactable = false;
        }
    }

    public bool HostGame(string matchID, GameObject player)
    {
        if (!matchIDs.Contains(matchID))
        {
            matchIDs.Add(matchID);
            matches.Add(new Match(matchID, player));
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool JoinGame(string matchID, GameObject player)
    {
        if (matchIDs.Contains(matchID))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].ID == matchID)
                {
                    matches[i].players.Add(player);
                    break;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public static string GetRandomID()
    {
        string ID = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            int rand = UnityEngine.Random.Range(0, 36);
            if (rand < 26)
            {
                ID += (char)(rand + 65);
            }
            else
            {
                ID += (rand - 26).ToString();
            }
        }
        return ID;
    }

    public void SpawnPlayerUIPrefab(Player player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<PlayerUI>().SetPlayer(player.playerDisplayName);
    }

    public void StartGame()
    {
        Player.localPlayer.BeginGame();
    }

    public void BeginGame(string matchID)
    {
        joinInput.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(false);
        GameObject newTurnManager = Instantiate(TurnManager);
        NetworkServer.Spawn(newTurnManager);
        newTurnManager.GetComponent<NetworkMatch>().matchId = matchID.ToGuid();
        TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();

        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].ID == matchID)
            {
                foreach (var player in matches[i].players)
                {
                    Player player1 = player.GetComponent<Player>();
                    turnManager.AddPlayer(player1);
                    player1.StartGame();
                    player1.canMove = true;
                }
                break;
            }
        }

    }
}

public static class MatchExtension
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hasBytes = provider.ComputeHash(inputBytes);

        return new Guid(hasBytes);
    }
}
