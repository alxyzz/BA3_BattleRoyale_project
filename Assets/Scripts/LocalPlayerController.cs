using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;
//using UnityEngine.UIElements;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class LocalPlayerController : NetworkBehaviour
{
    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
    }

    /// <summary>
    /// Called when the local player object is being stopped.
    /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStopLocalPlayer() { }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion

    [Header("Components")]
    [SerializeField] private Transform _thirdPersonRoot;
    [SerializeField] private Transform _firstPersonRoot;
    // [SerializeField] private AudioSource _soundPlayer;

    public Vector3 FirstPersonForward => _firstPersonRoot.forward;
    [SerializeField] private Transform _firstPersonArm;
    [SerializeField] private SkinnedMeshRenderer _fpSMR;
    [SerializeField] private SkinnedMeshRenderer _tpSMR;

    private CharacterMovement _charaMovement;
    public CharacterMovement CharaMovementComp => _charaMovement;
    private PlayerState _playerState;

    [Header("Settings")]
    [SerializeField] private float _mouseSensitivity = 2.0f;
    public float MouseSensitivityMultiplier { get; set; } = 1.0f;
    private float MouseSensitivity => _mouseSensitivity * MouseSensitivityMultiplier;
    // [SerializeField] private Transform _gunRoot;
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }

    private void Awake()
    {
        _charaMovement = GetComponent<CharacterMovement>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Start()
    {
        SetFirstPersonVisible(false);
        GetComponentInChildren<FpAnimEventHandler>().IsLocalPlayer = isLocalPlayer;
        if (isLocalPlayer)
        {
            SetThirdPersonVisible(false);

            //if (!isServer) // if the local client is not the host
            //    _thirdPersonRoot.gameObject.SetActive(false);

            Camera.main.transform.SetParent(_firstPersonRoot);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
            _firstPersonArm.SetParent(Camera.main.transform);

            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;


            // Crouch callback
            _charaMovement.OnStartCrouching += () => { UpdateCrouchCoroutine(1); };
            _charaMovement.OnEndCrouching += () => { UpdateCrouchCoroutine(-1); };

            // Die callback
            _playerState.onDied += Die;
        }        
        else
        {
            // Destroy(this.GetComponentInChildren<FpAnimEventHandler>());
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!GameState.HasBegun) return;
        if (!isLocalPlayer) return;
        UpdateShowStatisticsInput();
        if (!_playerState.IsAlive) return;

        UpdateRotationInput();
        UpdateMovementInput();
        UpdateCrouchingInput();
        UpdateWalkingInput();
        UpdateJumpingInput();
        UpdateFireInput();
        UpdateChangeWeaponInput();
        UpdateReloadInput();
        UpdateInspectInput();
        UpdateScopeInput();

        CheckInteractable();
        // test for sync
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            GetComponent<PlayerState>().CmdSetBodyColour(Color.red);
        }
        // test
        if (Input.GetKeyDown(KeyCode.K))
        {
            // MyNetworkManager.singleton.ServerChangeScene("MainMap");
            _playerState.ApplyDamage(100, _playerState, null, DamageType.POISON);
        }
    }

    private void UpdateRotationInput()
    {
        Yaw += Input.GetAxis("Mouse X") * MouseSensitivity;
        Pitch += Input.GetAxis("Mouse Y") * MouseSensitivity;
        Pitch = Mathf.Clamp(Pitch, -90, 90);

        _firstPersonRoot.localRotation = Quaternion.Euler(Pitch, 0, 0);
        transform.rotation = Quaternion.Euler(0, Yaw, 0);
    }

    private void UpdateMovementInput()
    {
        float axisH = Input.GetAxis("Horizontal"), axisV = Input.GetAxis("Vertical");
        _charaMovement.AddMovementInput(
            transform.rotation,
            new Vector2(axisH, axisV)
            );
    }

    #region Crouch
    private Coroutine _crouchCoroutine;
    private readonly Vector3 _standLocalPosition = new Vector3(0.0f, 1.7f, 0.0f);
    private readonly Vector3 _crouchLocalPosition = new Vector3(0.0f, 1.0f, 0.0f);
    private readonly float _crouchSpeed = 5.0f;
    private float _crouchValue = 0.0f;
    private void UpdateCrouchingInput()
    {
        if (Input.GetButtonDown("Crouch"))
            _charaMovement.Crouch();
        else if (Input.GetButtonUp("Crouch"))
            _charaMovement.Uncrouch();
    }
    public void UpdateCrouchCoroutine(float crouch)
    {
        if (null != _crouchCoroutine) StopCoroutine(_crouchCoroutine);
        _crouchCoroutine = StartCoroutine(UpdateCrouch(crouch));
    }
    IEnumerator UpdateCrouch(float crouch)
    {
        while ((crouch > 0 && _crouchValue < 1) || (crouch < 0 && _crouchValue > 0))
        {
            _crouchValue = Mathf.Clamp01(_crouchValue + crouch * _crouchSpeed * Time.deltaTime);
            _firstPersonRoot.localPosition = Vector3.Lerp(_standLocalPosition, _crouchLocalPosition, _crouchValue);
            yield return null;
        }
    }
    #endregion
    private void UpdateWalkingInput()
    {

        if (Input.GetButtonDown("Walk"))
        {
            _charaMovement.CmdSetIsWalking(true);
        }
        else if (Input.GetButtonUp("Walk"))
        {
            _charaMovement.CmdSetIsWalking(false);
        }
    }

    private void UpdateJumpingInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _charaMovement.Jump();
        }
    }
    [Header("Interaction")]
    [SerializeField] private LayerMask _interactionLayer;
    [SerializeField] private float _interactionDistance = 2.5f;
    private IInteractable _seeingInteractable;
    private void CheckInteractable()
    {
        bool result = Physics.SphereCast(
            _firstPersonRoot.position,
            0.3f,
            _firstPersonRoot.forward,
            out RaycastHit hit,
            _interactionDistance,
            _interactionLayer);
        if (result)
        {
            if (hit.transform.TryGetComponent(out IInteractable i))
            {
                if (_seeingInteractable == null)
                {
                    _seeingInteractable = i;
                    _seeingInteractable.StartBeingSeen();

                }
                else if (i != _seeingInteractable)
                {
                    _seeingInteractable.EndBeingSeen();
                    _seeingInteractable = i;
                    _seeingInteractable.StartBeingSeen();
                }
            }
            else
            {
                if (null != _seeingInteractable)
                {
                    _seeingInteractable.EndBeingSeen();
                    _seeingInteractable = null;
                }
            }
        }
        else if (null != _seeingInteractable)
        {

            _seeingInteractable.EndBeingSeen();
            _seeingInteractable = null;
        }

        if (null != _seeingInteractable && Input.GetButtonDown("Interact"))
        {
            _seeingInteractable.EndBeingSeen();
            _seeingInteractable.BeInteracted(_playerState);
            _seeingInteractable = null;
        }
    }


    #region Weapon
    private void UpdateFireInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _playerState.FireBurst();
        }
        else if (Input.GetButton("Fire1"))
        {
            _playerState.FireContinuously();
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            _playerState.FireStop();
        }
    }
    private void UpdateChangeWeaponInput()
    {
        float val = Input.GetAxisRaw("Mouse ScrollWheel");
        if (val != 0)
            _playerState.EquipScroll((int)val);
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            _playerState.EquipAt(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            _playerState.EquipAt(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            _playerState.EquipAt(2);
    }
    private void UpdateReloadInput()
    {
        if (Input.GetButtonDown("Reload"))
        {
            _playerState.StartReload();
        }
    }
    #endregion

    #region Statistics
    private void UpdateShowStatisticsInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UI_GameHUD.SetStatisticsShown(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            UI_GameHUD.SetStatisticsShown(false);
        }
    }
    #endregion

    #region Inspect
    private void UpdateInspectInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _playerState.Inspect();
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            GetComponentInChildren<FpAnimEventHandler>().ResetInspectMultiplier();
        }
    }
    #endregion
    [Header("Death")]
    [SerializeField] private Transform _spectator;
    [SerializeField] private float _deathLerpDuration = 1.5f;
    public void Die()
    {
        SetFirstPersonVisible(false);
        SetThirdPersonVisible(true);

        UI_GameHUD.SetUIEnabled(false);
        Camera.main.transform.GetComponent<CameraShake>().Stop();
        Camera.main.transform.GetChild(0).SetParent(Camera.main.transform.parent);
        Camera.main.transform.SetParent(null);
        StartCoroutine(UpdateCameraToSpectator(
            Camera.main.transform.position,
            Camera.main.transform.rotation));
    }
    private IEnumerator UpdateCameraToSpectator(Vector3 startPos, Quaternion startRot)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        float time = 0;
        while (time < _deathLerpDuration)
        {
            time = Mathf.Min(_deathLerpDuration, time + Time.deltaTime);
            Camera.main.transform.SetPositionAndRotation(
                Vector3.Lerp(startPos, _spectator.position, curve.Evaluate(time / _deathLerpDuration)),
                Quaternion.Slerp(startRot, _spectator.rotation, curve.Evaluate(time / _deathLerpDuration))
                );
            yield return null;
        }
        Camera.main.AddComponent<SpectatorCamera>();
    }

    private void UpdateScopeInput()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            _playerState.ToggleScope();
        }
    }

    public void SetFirstPersonVisible(bool visible)
    {

        _fpSMR.gameObject.layer = visible ? LayerMask.NameToLayer("Ignore Raycast") : LayerMask.NameToLayer("Disable Rendering");
    }
    public void SetThirdPersonVisible(bool visible)
    {
        _tpSMR.gameObject.layer = visible ? LayerMask.NameToLayer("Ignore Raycast") : LayerMask.NameToLayer("Disable Rendering");
    }
}
