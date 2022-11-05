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
    public static float GetCrosshairSpread()
    {
        return instance._crosshair.Spread;
    }
}
