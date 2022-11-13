using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    [SyncVar] public string matchID;
    public TextMesh NameDisplayText;
    [SyncVar(hook = "DisplayPlayerName")] public string playerDisplayName;

    private NetworkMatch networkMatch;

    public float speed = 6;
    Vector2 rotation;
    public bool canMove = false;
    // Start is called before the first frame update
    void Start()
    {
        rotation = Vector2.zero;
        networkMatch = GetComponent<NetworkMatch>();
        if (isLocalPlayer)
        {
            localPlayer = this;
            CmdSendName(MainMenu.instance.DisplayName);
        }
        else
        {
            MainMenu.instance.SpawnPlayerUIPrefab(this);
        }
    }

    [Command]
    public void CmdSendName(string name)
    {
        playerDisplayName = name;
    }

    public void DisplayPlayerName(string name, string playerName)
    {
        name = playerDisplayName;
        Debug.Log("Name " + name + " : " + playerName);
        NameDisplayText.text = playerName;
    }

    public void HostGame()
    {
        string ID = MainMenu.GetRandomID();
        CmdHostGame(ID);
    }

    [Command]
    public void CmdHostGame(string ID)
    {
        matchID = ID;
        if (MainMenu.instance.HostGame(ID, gameObject))
        {
            Debug.Log("Lobby was successfully created");
            networkMatch.matchId = ID.ToGuid();
            TargetHostGame(true, ID);
        }
        else
        {
            Debug.Log("Error occured");
            TargetHostGame(false, ID);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string ID)
    {
        matchID = ID;
        Debug.Log($"ID {matchID} == {ID}");
        MainMenu.instance.HostSuccess(success, ID);
    }

    public void JoinGame(string inputID)
    {
        CmdJoinGame(inputID);
    }

    [Command]
    public void CmdJoinGame(string ID)
    {
        matchID = ID;
        if (MainMenu.instance.JoinGame(ID, gameObject))
        {
            Debug.Log("Connected successfully");
            networkMatch.matchId = ID.ToGuid();
            TargetJoinGame(true, ID);
        }
        else
        {
            Debug.Log("Failed to connect");
            TargetJoinGame(false, ID);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string ID)
    {
        matchID = ID;
        Debug.Log($"ID {matchID} == {ID}");
        MainMenu.instance.JoinSuccess(success, ID);
    }

    public void BeginGame()
    {
        CmdBeginGame();
    }

    [Command]
    public void CmdBeginGame()
    {
        MainMenu.instance.BeginGame(matchID);
        Debug.Log("Game started");
    }

    public void StartGame()
    {
        TargetBeginGame();
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"ID {matchID} | begining");
        DontDestroyOnLoad(gameObject);
        MainMenu.instance.inGame = true;
        transform.localScale = new Vector3(1f, 1f, 1f);
        SceneManager.LoadScene("MainMap", LoadSceneMode.Additive);
    }



    // Update is called once per frame
    void Update()
    {
        if (hasAuthority && canMove)
        {
            float xMove = Input.GetAxis("Horizontal") * Time.deltaTime * 5f;
            float zMove = Input.GetAxis("Vertical") * Time.deltaTime * 5f;

            transform.Translate(xMove, 0, zMove);

            //Vector2 rotation = Vector2.zero;
            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");
            transform.eulerAngles = (Vector2)rotation * speed;

        }

        
        //public float speed = 3;
    }
}
