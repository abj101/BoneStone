using UnityEngine;

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
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnKnockback -= OnKnockback;
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

        // Left stick / WASD — movement only, never affects facing
        _rawInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        // Right stick — explicit aim, takes priority (gamepad)
        float aimX = Input.GetAxisRaw("RightStickHorizontal");
        float aimY = Input.GetAxisRaw("RightStickVertical");
        if (aimX * aimX + aimY * aimY > 0.04f)
        {
            Vector3 stickDir = new Vector3(aimX, 0f, aimY).ToIso();
            if (stickDir.sqrMagnitude > 0.001f)
                _lastFacingDir = stickDir.normalized;
            return;
        }

        // Mouse — aim toward cursor world position on ground plane (only when not using gamepad)
        if (_useMouseAim && _cam != null)
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
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

    private void UpdateMouseAimMode()
    {
        // Disable mouse aim when any gamepad control is received
        float aimX = Input.GetAxisRaw("RightStickHorizontal");
        float aimY = Input.GetAxisRaw("RightStickVertical");
        if (aimX * aimX + aimY * aimY > 0.04f)
            _useMouseAim = false;

        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + i)))
            {
                _useMouseAim = false;
                break;
            }
        }

        // Re-enable when keyboard or mouse is used
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            _useMouseAim = true;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            _useMouseAim = true;
    }

    private void Move()
    {
        if (_dash != null && _dash.IsDashing) return;

        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _knockbackDecay * Time.deltaTime);

        // Convert raw input to iso world direction
        Vector3 targetMove = Vector3.zero;
        if (_rawInput.sqrMagnitude > 0.001f)
            targetMove = _rawInput.ToIso().normalized * _speed;

        // Smooth the horizontal movement to avoid pivoting
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
