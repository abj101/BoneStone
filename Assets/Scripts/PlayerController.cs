using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _turnSpeed = 360f;
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
    private Vector3 _input;
    private Vector3 _verticalVelocity;
    private Vector3 _knockbackVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _dash = GetComponent<DashController>();
        _health = GetComponent<Health>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        // Movement is driven by CharacterController; prevent animations from moving the root (fixes floating)
        if (_animator != null)
            _animator.applyRootMotion = false;
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
        _input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        );
    }

    private void Move()
    {
        if (_dash != null && _dash.IsDashing) return;

        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _knockbackDecay * Time.deltaTime);

        Vector3 moveDirection = Vector3.zero;

        if (_input.sqrMagnitude > 0.001f)
        {
            moveDirection = _input.ToIso().normalized * _speed;
        }

        if (_knockbackVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 kb = _knockbackVelocity;
            kb.y = 0f;
            moveDirection += kb;
        }

        moveDirection += _verticalVelocity;

        _controller.Move(moveDirection * Time.deltaTime);
    }

    private void Look()
    {
        if (_input.sqrMagnitude < 0.001f) return;

        Vector3 isoDir = _input.ToIso();

        Quaternion targetRot = Quaternion.LookRotation(isoDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            _turnSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -2f;
        }
        else
        {
            _verticalVelocity.y += _gravity * Time.deltaTime;
        }
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        if (_input.sqrMagnitude < 0.001f)
        {
            _animator.SetFloat(MoveX, 0f);
            _animator.SetFloat(MoveY, 0f);
            return;
        }

        Vector3 worldDir = _input.ToIso().normalized;
        Vector3 localDir = transform.InverseTransformDirection(worldDir);
        _animator.SetFloat(MoveX, localDir.x);
        _animator.SetFloat(MoveY, localDir.z);
    }
}