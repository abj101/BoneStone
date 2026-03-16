using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _turnSpeed = 720f;
    [SerializeField] private float _moveSmoothTime = 0.08f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Knockback")]
    [SerializeField] private float _knockbackDecay = 10f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    [Header("Input")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _lookAction;

    private static readonly int MoveX = Animator.StringToHash("localMoveVectorX");
    private static readonly int MoveY = Animator.StringToHash("localMoveVectorY");

    private CharacterController _controller;
    private DashController _dash;
    private Health _health;
    private Camera _cam;
    private Plane _groundPlane = new Plane(Vector3.up, 0f);

    private Vector3 _rawInput;
    private Vector3 _smoothMove;
    private Vector3 _smoothMoveVel;
    private Vector3 _lastFacingDir;

    private Vector3 _verticalVelocity;
    private Vector3 _knockbackVelocity;

    private bool _useMouseAim = true;

    public Vector3 FacingDir => _lastFacingDir;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _dash = GetComponent<DashController>();
        _health = GetComponent<Health>();
        _cam = Camera.main;

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_animator != null)
            _animator.applyRootMotion = false;

        _lastFacingDir = transform.forward;
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnKnockback += OnKnockback;
        if (_moveAction != null)
            _moveAction.action.Enable();
        if (_lookAction != null)
            _lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnKnockback -= OnKnockback;
        if (_moveAction != null)
            _moveAction.action.Disable();
        if (_lookAction != null)
            _lookAction.action.Disable();
    }

    private void OnKnockback(Vector3 force)
    {
        _knockbackVelocity = force;
    }

    private void Update()
    {
        GatherInput();
        Move();
        Look();
        ApplyGravity();
        UpdateAnimator();
    }

    private void GatherInput()
    {
        UpdateMouseAimMode();

        // Move: Input System Vector2 (WASD / left stick)
        Vector2 move2 = Vector2.zero;
        if (_moveAction != null)
            move2 = _moveAction.action.ReadValue<Vector2>();

        // Safety fallback: if action wiring is missing/broken, still allow movement.
        if (move2.sqrMagnitude < 0.0001f)
            move2 = ReadKeyboardMoveFallback() + ReadGamepadMoveFallback();
        move2 = Vector2.ClampMagnitude(move2, 1f);
        _rawInput = new Vector3(move2.x, 0f, move2.y);

        // Look: gamepad right stick takes priority
        Vector2 look2 = Vector2.zero;
        if (_lookAction != null)
            look2 = _lookAction.action.ReadValue<Vector2>();

        if (look2.sqrMagnitude < 0.0001f)
            look2 = ReadGamepadLookFallback();

        if (look2.sqrMagnitude > 0.04f)
        {
            Vector3 stickDir = new Vector3(look2.x, 0f, look2.y).ToIso();
            if (stickDir.sqrMagnitude > 0.001f)
                _lastFacingDir = stickDir.normalized;
            return;
        }

        // Mouse: aim toward cursor world position on ground plane
        if (_useMouseAim && _cam != null && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);
            if (_groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldPoint = ray.GetPoint(enter);
                Vector3 toMouse = worldPoint - transform.position;
                toMouse.y = 0f;
                if (toMouse.sqrMagnitude > 0.25f)
                    _lastFacingDir = toMouse.normalized;
            }
        }
    }

    private static Vector2 ReadKeyboardMoveFallback()
    {
        if (Keyboard.current == null) return Vector2.zero;

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;

        return new Vector2(x, y).normalized;
    }

    private static Vector2 ReadGamepadMoveFallback()
    {
        if (Gamepad.current == null) return Vector2.zero;
        return Gamepad.current.leftStick.ReadValue();
    }

    private static Vector2 ReadGamepadLookFallback()
    {
        if (Gamepad.current == null) return Vector2.zero;
        return Gamepad.current.rightStick.ReadValue();
    }

    private void UpdateMouseAimMode()
    {
        // Disable mouse aim when gamepad right stick is used
        Vector2 look2 = _lookAction != null ? _lookAction.action.ReadValue<Vector2>() : Vector2.zero;
        if (look2.sqrMagnitude > 0.04f)
        {
            _useMouseAim = false;
            return;
        }
        // Re-enable when keyboard or mouse is used
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
            _useMouseAim = true;
        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame))
            _useMouseAim = true;
    }

    private void Move()
    {
        if (_dash != null && _dash.IsDashing) return;

        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _knockbackDecay * Time.deltaTime);

        Vector3 targetMove = Vector3.zero;
        if (_rawInput.sqrMagnitude > 0.001f)
            targetMove = _rawInput.ToIso().normalized * _speed;

        _smoothMove = Vector3.SmoothDamp(_smoothMove, targetMove, ref _smoothMoveVel, _moveSmoothTime);

        Vector3 finalMove = _smoothMove;

        if (_knockbackVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 kb = _knockbackVelocity;
            kb.y = 0f;
            finalMove += kb;
        }

        finalMove += _verticalVelocity;
        _controller.Move(finalMove * Time.deltaTime);
    }

    private void Look()
    {
        if (_lastFacingDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(_lastFacingDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            _turnSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity.y < 0)
            _verticalVelocity.y = -2f;
        else
            _verticalVelocity.y += _gravity * Time.deltaTime;
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        if (_rawInput.sqrMagnitude < 0.001f)
        {
            _animator.SetFloat(MoveX, 0f, 0.1f, Time.deltaTime);
            _animator.SetFloat(MoveY, 0f, 0.1f, Time.deltaTime);
            return;
        }

        Vector3 worldDir = _rawInput.ToIso().normalized;
        Vector3 localDir = transform.InverseTransformDirection(worldDir);
        _animator.SetFloat(MoveX, localDir.x, 0.1f, Time.deltaTime);
        _animator.SetFloat(MoveY, localDir.z, 0.1f, Time.deltaTime);
    }
}
