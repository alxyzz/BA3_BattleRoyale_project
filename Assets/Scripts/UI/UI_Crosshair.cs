using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Crosshair : MonoBehaviour
{
    [SerializeField] private RectTransform _left;
    [SerializeField] private RectTransform _right;
    [SerializeField] private RectTransform _up;
    [SerializeField] private RectTransform _down;

    private float _weaponSpread = 0;
    public float WeaponSpread 
    { 
        get { return _weaponSpread; }
        set
        {
            _weaponSpread = value;
            RefreshCrosshair();
        }
    }
    private float _movementSpread = 0;
    public float MovementSpread 
    {
        get { return _movementSpread; }
        set
        {
            _movementSpread = value;
            RefreshCrosshair();
        }
    }
    private float _fireSread = 0;
    private float FireSpread 
    { 
        get { return _fireSread; }
        set
        {
            _fireSread = value;
            RefreshCrosshair();
        }
    }
    private float Spread { get { return WeaponSpread + MovementSpread + FireSpread; } }

    private void RefreshCrosshair()
    {
        Vector3 vec = Vector3.zero;
        vec.x = -Spread;
        _left.localPosition = vec;
        vec.x = Spread;
        _right.localPosition = vec;
        vec.x = 0;
        vec.y = -Spread;
        _down.localPosition = vec;
        vec.y = Spread;
        _up.localPosition = vec;
    }
    [SerializeField] private AnimationCurve _defaultFireSpreadCurve;
    public void SetFireSpread(float amplitude, float duration)
    {
        SetFireSpread(amplitude, duration, _defaultFireSpreadCurve);
    }
    public void SetFireSpread(float amplitude, float duration, AnimationCurve curve)
    {
        if (_cUpdateFireSpread != null) StopCoroutine(_cUpdateFireSpread);
        _cUpdateFireSpread = StartCoroutine(UpdateFireSpread(amplitude, duration, curve));
    }
    private Coroutine _cUpdateFireSpread;
    private IEnumerator UpdateFireSpread(float amplitude, float duration, AnimationCurve curve)
    {
        if (duration > 0)
        {
            float time = 0;
            while (time < duration)
            {
                time = Mathf.Min(duration, time + Time.deltaTime);
                FireSpread = curve.Evaluate(time / duration) * amplitude;
                yield return null;
            }
        }
        FireSpread = 0;
    }
}
