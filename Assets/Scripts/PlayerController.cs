using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _turnSpeed = 360f;

    private Vector3 _input;

    private void Update()
    {
        GatherInput();
    }

    private void FixedUpdate()
    {
        Move();
        Look();
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
        if (_input.sqrMagnitude < 0.001f) return;

        Vector3 isoDir = _input.ToIso().normalized;

        _rb.MovePosition(
            _rb.position + isoDir * _speed * Time.fixedDeltaTime
        );
    }

    private void Look()
    {
        if (_input.sqrMagnitude < 0.001f) return;

        Vector3 isoDir = _input.ToIso();

        Quaternion targetRot = Quaternion.LookRotation(isoDir, Vector3.up);

        _rb.MoveRotation(
            Quaternion.RotateTowards(
                _rb.rotation,
                targetRot,
                _turnSpeed * Time.fixedDeltaTime
            )
        );
    }
}
