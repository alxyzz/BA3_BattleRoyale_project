using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZoneBehaviour : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;
        _currentRadius = 220;
        _currentCenter = transform.position;
        StartCoroutine(Shrinking());        
    }

    [SerializeField] private float _minRadius = 30;
    private float _currentRadius = 500;
    private float _targetRadius = 500;
    private float _initialScale;
    private Vector3 _currentCenter;
    private Vector3 _targetCenter;
    [SerializeField] private Vector2 _shrinkingRange = new Vector2(25, 50);
    [SerializeField] private Vector2 _pauseTimeRange = new Vector2(15, 30);
    [SerializeField] private float _shrinkingSpeed = 1.0f;
    [SerializeField] private int _damage = 5;
    IEnumerator Shrinking()
    {
        while (_currentRadius > _minRadius)
        {
            yield return new WaitForSeconds(Random.Range(_pauseTimeRange.x, _pauseTimeRange.y));

            // new target radius
            _targetRadius = Mathf.Max(_minRadius, _currentRadius - Random.Range(_shrinkingRange.x, _shrinkingRange.y));
            // new target center
            _targetCenter = PickNewCenter();

            // shrink
            while (_currentRadius > _targetRadius)
            {
                _currentRadius = Mathf.MoveTowards(_currentRadius, _targetRadius, _shrinkingSpeed * Time.deltaTime);
                _currentCenter = Vector3.MoveTowards(_currentCenter, _targetCenter, _shrinkingSpeed * Time.deltaTime);
                transform.position = _currentCenter;
                transform.localScale = new Vector3(_currentRadius, 1, _currentRadius);
                yield return null;
            }

        }
    }

    private Vector3 PickNewCenter()
    {
        float radius = _targetRadius - _currentRadius;
        return Random.insideUnitSphere * radius + _currentCenter;
    }

    private IEnumerator ApplyDamageToPlayer()
    {
        while (true)
        {
            Vector3 vec = Vector3.zero;
            foreach (var item in GameState.PlayerStates)
            {
                vec.x = item.transform.position.x;
                vec.z = item.transform.position.z;
                if ((vec - transform.position).sqrMagnitude > _currentRadius * _currentRadius)
                {
                    item.ApplyDamage(_damage, null, gameObject, DamageType.POISON);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
