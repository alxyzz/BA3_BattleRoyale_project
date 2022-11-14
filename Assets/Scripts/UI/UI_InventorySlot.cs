using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventorySlot : MonoBehaviour
{
    private UI_Widget _widget;
    [SerializeField] private TextMeshProUGUI _tmpWeaponName;
    Vector3 _originPosition;

    private void Awake()
    {
        _widget = GetComponent<UI_Widget>();
    }
    private void Start()
    {
        _tmpWeaponName.SetText("");
        _tmpWeaponName.color = Color.gray;
        _tmpWeaponName.fontSize = 28;
        Debug.Log(transform.localPosition);
        StartCoroutine(DelayInit());
    }
    private IEnumerator DelayInit()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(transform.localPosition);

        _originPosition = transform.localPosition;
    }
    public void SetNewName(string newName)
    {
        _widget.RenderOpacity = 0;
        transform.localPosition = _originPosition + Vector3.right * 30;
        _tmpWeaponName.SetText(newName);
        _widget.Fade(1, 0.25f);
        _widget.Translate(Vector3.left * 30, 0.25f);
    }

    private float _activeValue;
    private float _activeChangeSpeed;
    private Coroutine _coroutineUpdateActive;
    public void SetWeaponActive(bool active)
    {
        _activeChangeSpeed = active ? 6 : -5;
        if (null == _coroutineUpdateActive) StartCoroutine(UpdateActive());
    }
    private IEnumerator UpdateActive()
    {
        while (true)
        {
            if (_activeChangeSpeed > 0 && _activeValue >= 1) break;
            if (_activeChangeSpeed < 0 && _activeValue <= 0) break;
            _activeValue = Mathf.Clamp01(_activeValue + _activeChangeSpeed * Time.deltaTime);

            _tmpWeaponName.color = Color.Lerp(Color.gray, Color.white, AnimationCurve.EaseInOut(0, 0, 1, 1).Evaluate(_activeValue));
            _tmpWeaponName.fontSize = (int)Mathf.Lerp(28, 54, AnimationCurve.EaseInOut(0, 0, 1, 1).Evaluate(_activeValue));
            yield return null;
        }
    }
}
