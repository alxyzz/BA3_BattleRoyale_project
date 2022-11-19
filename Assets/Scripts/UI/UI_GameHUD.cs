using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameHUD : MonoBehaviour
{
    private static UI_GameHUD instance;
    public static UI_GameHUD Instance => instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {        
        ClearInteractionHint();
        SetScopeActive(false);
    }

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI _tmpBeginplayCountdown;
    [SerializeField] private TextMeshProUGUI _tmpZoneCountdown;
    public void UpdateConnectedPlayerNum(int num, int max)
    {
        _tmpBeginplayCountdown.SetText($"Waiting for other players...{num}/{max}");
    }
    public static void SetCountdown(string str)
    {
        instance._tmpBeginplayCountdown.SetText(str);
    }
    public void SetZoneCountdown(int val, bool urgent = false)
    {
        _tmpZoneCountdown.color = urgent ? Color.red : Color.white;
        if (null != _cZoneCountdown) StopCoroutine(_cZoneCountdown);
        _cZoneCountdown = StartCoroutine(ZoneCountdown(val));
    }
    Coroutine _cZoneCountdown;
    private IEnumerator ZoneCountdown(int val)
    {
        while (val > 0)
        {
            _tmpZoneCountdown.SetText(val.ToString() + " s");
            val--;
            yield return new WaitForSeconds(1.0f);
        }
    }

    [Header("Interaction")]
    [SerializeField] private TextMeshProUGUI _tmpInteraction;

    public static void AddInteractionHint(string content)
    {
        instance._tmpInteraction.SetText(content);
    }
    public static void ClearInteractionHint()
    {
        instance._tmpInteraction.SetText("");
    }

    [Header("Scope")]
    [SerializeField] private GameObject _objScope;
    public static void SetScopeActive(bool active)
    {
        instance._objScope.SetActive(active);
    }
    [Header("Opposite Name")]
    [SerializeField] private TextMeshProUGUI _tmpAimedPlayerName;
    [SerializeField] private LayerMask _aimLayerMask;
    public void SetAimedPlayerName(string nickname)
    {
        instance._tmpAimedPlayerName.SetText(nickname);
    }

    [Header("Inventory")]
    [SerializeField] private UI_Panel_Inventory _inventory;
    public static void SetNewWeapon(int index, string newName)
    {
        instance._inventory.SetNewWeapon(index, newName);
    }
    public static void ActiveInventorySlot(int index)
    {
        instance._inventory.ActiveSlot(index);
    }

    [Header("Ammo")]
    [SerializeField] private GameObject _pnlAmmo;
    [SerializeField] private TextMeshProUGUI _tmpAmmo;
    [SerializeField] private TextMeshProUGUI _tmpBackupAmmo;
    public static void SetAmmo(int val)
    {
        instance._tmpAmmo.SetText(val.ToString());
    }
    public static void SetBackupAmmo(int val)
    {
        instance._tmpBackupAmmo.SetText(val.ToString());
    }

    [Header("Crosshair")]
    [SerializeField] private UI_Crosshair _crosshair;
    public static void SetCrosshairActive(bool active)
    {
        instance._crosshair.gameObject.SetActive(active);
    }
    public static void SetCrosshairWeaponSpread(float pixel)
    {
        if (instance._crosshair.gameObject.activeSelf)
        {
            instance._crosshair.WeaponSpread = pixel;
        }
    }
    public static void SetCrosshairMovementSpread(float pixel)
    {
        if (instance._crosshair.gameObject.activeSelf)
        {
            instance._crosshair.MovementSpread = pixel;
        }
    }
    public static void SetCrosshairFireSpread(float pixel, float duration)
    {
        if (instance._crosshair.gameObject.activeSelf)
        {
            instance._crosshair.SetFireSpread(pixel, duration);
        }
    }

    [Header("Personal")]
    [SerializeField] private GameObject _pnlPersonal;
    [SerializeField] private TextMeshProUGUI _tmpHealth;
    [SerializeField] private Color _hpColor1 = Color.white;
    [SerializeField] private Color _hpColor2 = Color.yellow;
    [SerializeField] private Color _hpColor3 = Color.red;
    [SerializeField] private TextMeshProUGUI _tmpArmor;
    public void SetHealth(int val)
    {
        instance._tmpHealth.SetText(val.ToString());
        instance._tmpHealth.color = val >= 50 ? instance._hpColor1 :
            (val >= 20 ? instance._hpColor2 : instance._hpColor3);
    }
    public static void SetArmor(int val)
    {
        instance._tmpArmor.SetText(val.ToString());
    }

    [Header("Damaged")]
    [SerializeField] private UI_Game_Damaged _damaged;
    public void RegisterPlayerTransform(Transform player)
    {
        _damaged.PlayerTransform = player;
    }
    public void SetDamaged(Transform instigator)
    {
        _damaged.SetDamaged(instigator);
    }

    [Header("Kill Message")]
    [SerializeField] private UI_Game_KillMsg _killMsg;
    public static void AddKillMessage(string killerName, string objectName, Sprite icon, DamageType type)
    {
        instance._killMsg.AddKillMessage(killerName, objectName, icon, type);
    }
    [Header("Statistics")]
    [SerializeField] private UI_Panel_Statistics _statistics;
    public void SetStatisticsShown(bool shown)
    {
        _statistics.SetShown(shown);
    }
    public void AddPlayerToStatistics(uint netId)
    {
        _statistics.AddPlayerSlot(netId);
    }
    public void RemovePlayerFromStatistics(uint netId)
    {
        _statistics.RemovePlayerSlot(netId);
    }
    //private void InitPlayerStatistics()
    //{
    //    foreach (var item in GameState.Instance._playerNetIds)
    //    {
    //        AddPlayerToStatistics(item);
    //    }
    //}

    [Header("Round End")]
    [SerializeField] private GameObject _winnerPanel;
    [SerializeField] private RawImage _imgWinnerIcon;
    [SerializeField] private TextMeshProUGUI _tmpWinnerName;
    [SerializeField] private Button _btnReturnToLobby;

    public static void ShowWinner(PlayerState ps)
    {
        instance._winnerPanel.SetActive(true);

        Callback<AvatarImageLoaded_t>.Create(instance.OnWinnerIconLoaded);
        int ImageID = SteamFriends.GetLargeFriendAvatar(ps.SteamId);
        if (ImageID != -1) instance._imgWinnerIcon.texture = SteamLobby.GetSteamImageAsTexture(ImageID);

        instance._tmpWinnerName.SetText(SteamFriends.GetFriendPersonaName(ps.SteamId));
        instance._btnReturnToLobby.interactable =
            SteamMatchmaking.GetLobbyOwner(SteamLobby.Instance.CurrentLobbyId) == SteamUser.GetSteamID();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void OnWinnerIconLoaded(AvatarImageLoaded_t callback)
    {
        instance._imgWinnerIcon.texture = SteamLobby.GetSteamImageAsTexture(callback.m_iImage);
    }
    public void OnClickReturnToLobby()
    {
        // SteamLobby.SceneToLoad = "Lobby";
        MyNetworkManager.singleton.ServerChangeScene("Lobby");
    }
    public static void SetUIEnabled(bool enabled)
    {
        instance._pnlAmmo.SetActive(enabled);
        instance._pnlPersonal.SetActive(enabled);
        instance._inventory.gameObject.SetActive(enabled);
        instance._crosshair.gameObject.SetActive(enabled);
    }

    private void FixedUpdate()
    {
        if (null == GameState.Instance) return;
        if (GameState.Instance.Stage != GameStage.PLAYING) return;

        // check aiming player
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward,
            out RaycastHit hit,
            150,
            _aimLayerMask))
        {
            // Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.TryGetComponent(out PlayerState ps))
            {
                SetAimedPlayerName(ps.Nickname);
                return;
            }            
        }
        SetAimedPlayerName("");
    }
}
