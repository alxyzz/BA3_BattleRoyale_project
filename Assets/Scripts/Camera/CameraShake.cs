using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private AnimationCurve _defaultShakePitchCurve;
    [SerializeField] private AnimationCurve _defaultShakeYawCurve;
    [SerializeField] private AnimationCurve _defaultShakeRollCurve = AnimationCurve.Constant(0, 1, 0);
    public void Shake(float duration)
    {
        Shake(duration, _defaultShakePitchCurve, _defaultShakeYawCurve, _defaultShakeRollCurve);
    }
    public void Shake(float duration, AnimationCurve pitchCurve, AnimationCurve yawCurve, AnimationCurve rollCurve)
    {
        Stop();
        _cUpdateShake = StartCoroutine(UpdateShake(duration, pitchCurve, yawCurve, rollCurve));
    }
    public void Stop()
    {
        if (_cUpdateShake != null) StopCoroutine(_cUpdateShake);
        if (_cRecovery != null) StopCoroutine(_cRecovery);
    }
    private Coroutine _cUpdateShake;
    private IEnumerator UpdateShake(float duration, AnimationCurve pitchCurve, AnimationCurve yawCurve, AnimationCurve rollCurve)
    {
        if (duration > 0)
        {
            float time = 0;
            Vector3 start = transform.localRotation.eulerAngles;
            while (time < duration)
            {
                time = Mathf.Min(duration, time + Time.deltaTime);
                transform.localRotation = Quaternion.Euler(
                    start.x + pitchCurve.Evaluate(time / duration),
                    start.y + yawCurve.Evaluate(time / duration),
                    start.z + rollCurve.Evaluate(time / duration));
                yield return null;
            }
        }

        _cRecovery = StartCoroutine(Recovery(Quaternion.identity, _recoveryDutaion, AnimationCurve.EaseInOut(0,0,1,1)));
    }
    private Coroutine _cRecovery;
    private IEnumerator Recovery(Quaternion target, float duration, AnimationCurve curve)
    {
        if (duration > 0)
        {
            float time = 0;
            Quaternion start = transform.localRotation;
            while (time < duration)
            {
                time = Mathf.Min(duration, time + Time.deltaTime);
                transform.localRotation = Quaternion.SlerpUnclamped(start, target, curve.Evaluate(time / duration));
                yield return null;
            }
        }
        transform.localRotation = target;
    }

    private float _recoveryDutaion;
    public void ShakeTo(Quaternion target, float shakeDuration, float recoveryDuration)
    {
        transform.localRotation = target;
        Shake(shakeDuration);
        _recoveryDutaion = recoveryDuration;
    }
}
