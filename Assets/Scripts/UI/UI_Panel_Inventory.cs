using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Panel_Inventory : UI_Widget
{
    private UI_InventorySlot[] _slots;
    private int _currentIndex;
    private bool _hasShown;
    protected override void Awake()
    {
        base.Awake();
        _slots = GetComponentsInChildren<UI_InventorySlot>();
    }
    private void Start()
    {
        _currentIndex = -1;
        _hasShown = false;
        RenderOpacity = 0;
        StartCoroutine(DelayInit());
    }
    private IEnumerator DelayInit()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        foreach (var item in _slots)
        {
            item.InitPosition();
        }
        // UI_GameHUD.SetUIEnabled(false);
    }
    public void SetNewWeapon(int index, string newName)
    {
        Appear();
        _slots[index].SetNewName(newName);
    }
    public void ActiveSlot(int index)
    {
        Appear();
        if (_currentIndex >= 0 && _currentIndex < _slots.Length) _slots[_currentIndex].SetWeaponActive(false);
        _currentIndex = index;
        _slots[_currentIndex].SetWeaponActive(true);
    }
    //public void ActivePrevious()
    //{
    //    Appear();
    //    ActiveSlot((_currentIndex + _slots.Length - 1) % _slots.Length);
    //}
    //public void ActiveNext()
    //{
    //    Appear();
    //    ActiveSlot((_currentIndex + 1) % _slots.Length);
    //}

    private void Appear()
    {
        if (!_hasShown)
        {
            _hasShown = true;
            Fade(1, 0.25f);
        }
        if (_coroutineDisappearTimer != null) StopCoroutine(_coroutineDisappearTimer);
        _coroutineDisappearTimer = StartCoroutine(DisappearTimer());
    }
    private Coroutine _coroutineDisappearTimer;
    private IEnumerator DisappearTimer()
    {
        yield return new WaitForSeconds(3.0f);
        _hasShown = false;
        Fade(0, 0.15f);
    }
}
