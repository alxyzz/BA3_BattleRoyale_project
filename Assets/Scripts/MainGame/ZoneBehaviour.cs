using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class ZoneBehaviour : NetworkBehaviour
{
    Camera _mainCamera;
    Volume _postprocessVolume;
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _postprocessVolume = _mainCamera.GetComponent<Volume>();
        _currentRadius = transform.localScale.x;
        _currentCenter = transform.position;
        if (!isServer) return;
        GameState.Instance.onGameStarted += () =>
        {
            StartCoroutine(Shrinking());
            StartCoroutine(ApplyDamageToPlayer());
        };    
    }

    [SerializeField] private float _minRadius = 30;
    private float _currentRadius = 500;
    private float _initialScale;
    private Vector3 _currentCenter;
    [SerializeField] private Vector2 _changingRange = new Vector2(20, 40);
    [SerializeField] private Vector2Int _pauseTimeRange = new Vector2Int(15, 30);
    [SerializeField] private float _shrinkingDuration = 30f;
    [SerializeField] private int _baseDamage = 1;
    [SerializeField] private int _addedDamage = 10;
    IEnumerator Shrinking()
    {
        yield return new WaitForSeconds(10.0f);
        while (_currentRadius > _minRadius)
        {
            int pauseTime = Random.Range(_pauseTimeRange.x, _pauseTimeRange.y);
            MasterUIManager.AddPopupHint($"{pauseTime} seconds to restrict the play area!");
            UI_GameHUD.SetZoneCountdown(pauseTime);
            yield return new WaitForSeconds(pauseTime);

            float time = 0;
            float speed = 1 / _shrinkingDuration;
            float startRadius = _currentRadius;
            Vector3 startCenter = _currentCenter;

            float change = Random.Range(_changingRange.x, _changingRange.y);
            // new target radius
            float targetRadius = Mathf.Max(_minRadius, _currentRadius - change);
            // new target center
            Vector3 targetCenter = PickNewCenter(change);

            UI_GameHUD.SetZoneCountdown((int)_shrinkingDuration, true);
            // shrink
            while (time < 1)
            {
                time = Mathf.Min(1, time + Time.deltaTime * speed);
                _currentRadius = Mathf.Lerp(startRadius, targetRadius, time);
                _currentCenter = Vector3.Lerp(startCenter, targetCenter, time);

                transform.position = _currentCenter;
                transform.localScale = new Vector3(_currentRadius, 1, _currentRadius);
                yield return null;
            }
        }
    }

    private Vector3 PickNewCenter(float radius)
    {
        return Random.insideUnitSphere * radius + _currentCenter;
    }

    private IEnumerator ApplyDamageToPlayer()
    {
        while (true)
        {
            Vector3 vec = Vector3.zero;
            foreach (var item in GameState.Instance.PlayerStates)
            {
                vec.x = item.transform.position.x;
                vec.z = item.transform.position.z;
                if ((vec - transform.position).sqrMagnitude > _currentRadius * _currentRadius)
                {                    
                    item.ApplyDamage((int)((220 - _currentRadius) / 220.0f * _addedDamage) + _baseDamage, null, gameObject, DamageType.POISON);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void FixedUpdate()
    {
        Vector3 vec = new Vector3(_mainCamera.transform.position.x, 0, _mainCamera.transform.position.z);
        if ((vec - transform.position).sqrMagnitude > _currentRadius * _currentRadius)
        {
            _postprocessVolume.weight = 1;
        }
        else
        {
            _postprocessVolume.weight = 0;
        }
    }
}
