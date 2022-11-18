using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    [SerializeField] private float _mouseSensitivity = 2.0f;
    private Camera _camera;
    private Vector3 _dir;

    private float _stateAlpha;
    private AnimationCurve _fovCurve = AnimationCurve.EaseInOut(0, 60, 1, 80);
    private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0, 5, 1, 20);
    private readonly float _changeSpeed = 3f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        if (GameState.Instance.Stage != GameStage.PLAYING) return;

        // Update State
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (_stateAlpha < 1)
                _stateAlpha = Mathf.Min(_stateAlpha + _changeSpeed * Time.deltaTime, 1);
        }
        else if (_stateAlpha > 0)
            _stateAlpha = Mathf.Max(_stateAlpha - _changeSpeed * Time.deltaTime, 0);

        // Update Rotation
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * _mouseSensitivity, Space.World);
        transform.Rotate(transform.right, Input.GetAxis("Mouse Y") * _mouseSensitivity, Space.World);

        // Update Movement
        _dir =
            transform.right * Input.GetAxis("Horizontal") +
            transform.forward * Input.GetAxis("Vertical") +
            Vector3.up * Input.GetAxis("Up");
        transform.Translate(_dir * _speedCurve.Evaluate(_stateAlpha) * Time.deltaTime, Space.World);

        // Update FOV
        _camera.fieldOfView = _fovCurve.Evaluate(_stateAlpha);
    }

}
