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

    public float WeaponSpread { get; set; }
    public float MovementSpread { get; set; }
    public float Spread { get { return WeaponSpread + MovementSpread; } }

    private void Update()
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
    //public void SetSpread(float pixel)
    //{
    //    Vector3 vec = Vector3.zero;
    //    vec.x = -pixel;
    //    _left.localPosition = vec;
    //    vec.x = pixel;
    //    _right.localPosition = vec;
    //    vec.x = 0;
    //    vec.y = -pixel;
    //    _down.localPosition = vec;
    //    vec.y = pixel;
    //    _up.localPosition = vec;
    //}
    //private void Start()
    //{
    //    SetSpread(100);
    //}
}
