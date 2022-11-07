using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        ClearInteractionHint();
    }

    [Header("Interaction")]
    [SerializeField] private Text _interactionHint;

    public static void AddInteractionHint(string content)
    {
        instance._interactionHint.text = content;
    }
    public static void ClearInteractionHint()
    {
        instance._interactionHint.text = "";
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
    //public static void ActiveInventoryPrevious()
    //{
    //    instance._inventory.ActivePrevious();
    //}
    //public static void ActiveInventoryNext()
    //{
    //    instance._inventory.ActiveNext();
    //}

    [Header("Ammo")]
    [SerializeField] private Text _ammo;
    [SerializeField] private Text _backupAmmo;
    public static void SetAmmo(int val)
    {
        instance._ammo.text = val.ToString();
    }
    public static void SetBackupAmmo(int val)
    {
        instance._backupAmmo.text = val.ToString();
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

    [Header("Statistics")]
    [SerializeField] private Text _txtHealth;
    [SerializeField] private Color _hpColor1 = Color.white;
    [SerializeField] private Color _hpColor2 = Color.yellow;
    [SerializeField] private Color _hpColor3 = Color.red;
    [SerializeField] private Text _txtArmor;
    public static void SetHealth(int val)
    {
        instance._txtHealth.text = val.ToString();
        instance._txtHealth.color = val >= 50 ? instance._hpColor1 : 
            (val >= 20 ? instance._hpColor2 : instance._hpColor3);
    }
    public static void SetArmor(int val)
    {
        instance._txtArmor.text = val.ToString();
    }
}
