using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInputMode
{
    GameOnly,
    UIOnly,
    GameAndUI
}
public class InputManager
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get 
        {
            if (instance == null)
                instance = new InputManager();
            return instance; 
        }
    }

    private EInputMode _inputMode = EInputMode.GameOnly;
    public EInputMode InputMode => _inputMode;
    public void SetInputMode(EInputMode mode)
    {
        _inputMode = mode;
        switch (mode)
        {
            case EInputMode.GameOnly:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case EInputMode.UIOnly:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case EInputMode.GameAndUI:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                break;
            default:
                break;
        }
    }

}
