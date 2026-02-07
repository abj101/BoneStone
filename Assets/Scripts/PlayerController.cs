using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private HealthSystem _healthSystem;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _turnSpeed = 360;
    private Vector3 _input;

    private void Awake()
    {
        if (_healthSystem == null)
            _healthSystem = GetComponent<HealthSystem>();
        if (_rb != null)
            _rb.constraints |= RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (_healthSystem != null && !_healthSystem.IsAlive)
            return;
        GatherInput();
        Look();
    }

    private void FixedUpdate()
    {
        if (_healthSystem != null && (!_healthSystem.IsAlive || _healthSystem.IsBeingKnockedBack))
            return;
        Move();
    }

    private void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void Look()
    {
        if (_input == Vector3.zero) return;

        var rot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
    }

    private void Move()
    {
        _rb.MovePosition(transform.position + transform.forward * _input.normalized.magnitude * _speed * Time.deltaTime);
    }
}