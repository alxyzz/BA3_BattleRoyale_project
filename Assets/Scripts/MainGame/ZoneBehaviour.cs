using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class ZoneBehaviour : NetworkBehaviour
{
    public override void OnStartServer()
    {
        _currentRadius = transform.localScale.x;
        _currentCenter = transform.position;

        LocalGame.Instance.onServerGameStarted += () =>
        {
            StartCoroutine(Shrinking());
            StartCoroutine(ApplyDamageToPlayers());
        };
    }
    private Camera _mainCamera;
    private Vector3 _mainCameraPos = Vector3.zero;
    private Volume _postprocessVolume;
    [SyncVar] private float _currentRadius;
    [SyncVar] private Vector3 _currentCenter;
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _postprocessVolume = _mainCamera.GetComponent<Volume>();
    }

    [SerializeField] private float _minRadius = 30;
    [SerializeField] private Vector2 _changingRange = new Vector2(50, 60);
    [SerializeField] private Vector2Int _pauseTimeRange = new Vector2Int(20, 30);
    [SerializeField] private float _initialWaitingTime = 10.0f;
    [SerializeField] private int _shrinkingDuration = 30;
    [SerializeField] private int _baseDamage = 1;
    [SerializeField] private int _addedDamage = 10;
    IEnumerator Shrinking()
    {
        yield return new WaitForSeconds(_initialWaitingTime);
        while (_currentRadius > _minRadius)
        {
            int pauseTime = Random.Range(_pauseTimeRange.x, _pauseTimeRange.y);
            RpcPopupHint($"{pauseTime} seconds to restrict the play area!");
            RpcZoneCountdown(pauseTime, false);
            yield return new WaitForSeconds(pauseTime);

            float time = 0;
            float speed = 1.0f / _shrinkingDuration;
            float startRadius = _currentRadius;
            Vector3 startCenter = _currentCenter;

            float change = Mathf.Min(_currentRadius - _minRadius, Random.Range(_changingRange.x, _changingRange.y));
            // new target radius
            float targetRadius = _currentRadius - change;
            // new target center
            Vector3 targetCenter = PickNewCenter(change);

            RpcZoneCountdown(_shrinkingDuration, true);
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

    private IEnumerator ApplyDamageToPlayers()
    {
        while (true)
        {
            Vector3 vec = Vector3.zero;
            foreach (var item in GameState.Instance.GetPlayerStateList())
            {
                vec.x = item.transform.position.x;
                vec.z = item.transform.position.z;
                if (Vector3.Distance(vec, transform.position) > _currentRadius)
                {                    
                    item.ApplyDamage((int)((220 - _currentRadius) / 220.0f * _addedDamage) + _baseDamage, null, gameObject, DamageType.POISON);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
    [ClientRpc]
    private void RpcPopupHint(string str)
    {
        MasterUIManager.AddPopupHint(str);
    }
    [ClientRpc]
    private void RpcZoneCountdown(int val, bool urgent)
    {
        UI_GameHUD.Instance.SetZoneCountdown(val, urgent);
    }
    private void FixedUpdate()
    {
        _mainCameraPos.x = _mainCamera.transform.position.x;
        _mainCameraPos.z = _mainCamera.transform.position.z;
        if (Vector3.Distance(_mainCameraPos, transform.position) > _currentRadius)
        {
            _postprocessVolume.weight = 1;
        }
        else
        {
            _postprocessVolume.weight = 0;
        }
    }
}
