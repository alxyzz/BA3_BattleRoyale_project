using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Widget : MonoBehaviour
{
    private RectTransform _transform;
    private CanvasGroup _canvasGroup;
    public float RenderOpacity
    {
        get { return _canvasGroup.alpha; }
        set { _canvasGroup.alpha = value; }
    }
    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    #region Fade
    public void Fade(float target, float duration, Action onFinished = null)
    {
        Fade(target, duration, AnimationCurve.EaseInOut(0, 0, 1, 1), onFinished);       
    }
    public void Fade(float target, float duration, AnimationCurve curve, Action onFinished = null)
    {
        if (_coroutineUpdateFade != null) StopCoroutine(_coroutineUpdateFade);
        _coroutineUpdateFade = StartCoroutine(UpdateFade(target, duration, curve, onFinished));
    }
    Coroutine _coroutineUpdateFade;
    private IEnumerator UpdateFade(float target, float duration, AnimationCurve curve, Action onFinished)
    {
        if (duration > 0)
        {
            float time = 0;
            float start = RenderOpacity;
            while (time < duration)
            {
                time = Mathf.Min(duration, time + Time.deltaTime);
                RenderOpacity = Mathf.Lerp(start, target, curve.Evaluate(time / duration));
                yield return null;
            }
        }
        RenderOpacity = target;
        onFinished?.Invoke();
    }
    #endregion

    #region Translate
    public void Translate(Vector3 translation, float duration, Action onFinished = null)
    {
        Translate(translation, duration, AnimationCurve.EaseInOut(0, 0, 1, 1), onFinished);
    }
    public void Translate(Vector3 translation, float duration, AnimationCurve curve, Action onFinished = null)
    {
        if (_coroutineUpdateTranslate != null) StopCoroutine(_coroutineUpdateTranslate);
        _coroutineUpdateTranslate = StartCoroutine(UpdateTranslate(translation, duration, curve, onFinished));
    }
    Coroutine _coroutineUpdateTranslate;
    private IEnumerator UpdateTranslate(Vector3 translation, float duration, AnimationCurve curve, Action onFinished)
    {
        
        Vector3 target = transform.localPosition + translation;
        if (duration > 0)
        {
            float time = 0;
            Vector3 start = transform.localPosition;
            while (time < duration)
            {
                time = Mathf.Min(duration, time + Time.deltaTime);
                transform.localPosition = Vector3.Lerp(start, target, curve.Evaluate(time / duration));
                yield return null;
            }
        }
        transform.localPosition = target;
        onFinished?.Invoke();
    }
    #endregion
}
