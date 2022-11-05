using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    }
}
