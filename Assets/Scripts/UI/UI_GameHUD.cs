using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameHUD : MonoBehaviour
{
    private static UI_GameHUD instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        ClearInteractionHint();
        SetUIEnabled(false);
    }

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI _tmpCountdown;
    public static void SetCountdown(string str)
    {
        instance._tmpCountdown.SetText(str);
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
    public static void SetCrosshairWeaponSpread(float pixel)
    {
        instance._crosshair.WeaponSpread = pixel;
    }
    public static void SetCrosshairMovementSpread(float pixel)
    {
        instance._crosshair.MovementSpread = pixel;
    }
    public static void SetCrosshairFireSpread(float pixel, float duration)
    {
        instance._crosshair.SetFireSpread(pixel, duration);
    }

    [Header("Personal")]
    [SerializeField] private GameObject _pnlPersonal;
    [SerializeField] private TextMeshProUGUI _tmpHealth;
    [SerializeField] private Color _hpColor1 = Color.white;
    [SerializeField] private Color _hpColor2 = Color.yellow;
    [SerializeField] private Color _hpColor3 = Color.red;
    [SerializeField] private TextMeshProUGUI _tmpArmor;
    public static void SetHealth(int val)
    {
        instance._tmpHealth.SetText(val.ToString());
        instance._tmpHealth.color = val >= 50 ? instance._hpColor1 : 
            (val >= 20 ? instance._hpColor2 : instance._hpColor3);
    }
    public static void SetArmor(int val)
    {
        instance._tmpArmor.SetText(val.ToString());
    }

    [Header("Statistics")]
    [SerializeField] private UI_Panel_Statistics _statistics;
    public static void SetStatisticsShown(bool shown)
    {
        instance._statistics.SetShown(shown);
    }
    public static void AddPlayerToStatistics(PlayerState ps)
    {
        instance._statistics.AddPlayerSlot(ps);
    }
    public static void RemovePlayerFromStatistics(PlayerState ps)
    {
        instance._statistics.RemovePlayerSlot(ps);
    }
    //public static void ReorderStatistics()
    //{
    //    instance._statistics.Reorder();
    //}
    public static void SetUIEnabled(bool enabled)
    {
        instance._pnlAmmo.SetActive(enabled);
        instance._pnlPersonal.SetActive(enabled);
        instance._inventory.gameObject.SetActive(enabled);
        instance._crosshair.gameObject.SetActive(enabled);
    }
}
