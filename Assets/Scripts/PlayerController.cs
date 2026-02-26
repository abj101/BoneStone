using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _turnSpeed = 360f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _controller;
    private KnockbackController _knockback;
    private Vector3 _input;
    private Vector3 _verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _knockback = GetComponent<KnockbackController>();
    }

    private void Update()
    {
        GatherInput();
        Move();
        Look();
        ApplyGravity();
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
        Vector3 moveDirection = Vector3.zero;

        if (_input.sqrMagnitude > 0.001f)
        {
            moveDirection = _input.ToIso().normalized * _speed;
        }

        // Add knockback
        if (_knockback != null)
        {
            moveDirection += _knockback.KnockbackVelocity;
        }

        // Add gravity
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
}