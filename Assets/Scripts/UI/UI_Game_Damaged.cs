using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game_Damaged : MonoBehaviour
{
    private Image _imgDamaged;
    public Transform PlayerTransform { get; set; }
    public Transform Instigator { get; set; }
    private Vector3 Direction
    {
        get
        {
            if (Instigator == null) return PlayerTransform.forward * -1;
            else
            {
                Vector3 result = new Vector3(Instigator.position.x - PlayerTransform.position.x, 0, Instigator.position.z - PlayerTransform.position.z);
                if (result == Vector3.zero) return PlayerTransform.forward * -1;
                else return result;
            }
        }
    }
    private void Awake()
    {
        _imgDamaged = GetComponent<Image>();            
    }
    private void Start()
    {
        _imgDamaged.CrossFadeAlpha(0, 0, true);
    }
    private void Update()
    {
        if (null == GameState.Instance) return;
        if (GameState.Instance.Stage != GameStage.PLAYING) return;
        transform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Direction, PlayerTransform.forward, Vector3.up));
    }
    public void SetDamaged(Transform instigator)
    {
        Instigator = instigator;
        _imgDamaged.CrossFadeAlpha(0.8f, 0, true);
        if (null != _cDisappear) StopCoroutine(_cDisappear);
        _cDisappear = StartCoroutine(Disappear(2.0f, 1f));
    }
    private Coroutine _cDisappear;
    private IEnumerator Disappear(float delay, float duration)
    {
        yield return new WaitForSecondsRealtime(delay);
        _imgDamaged.CrossFadeAlpha(0, duration, true);
    }
}
