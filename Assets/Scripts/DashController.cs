using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DashController : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.15f;

    [Header("Invulnerability")]
    [SerializeField] private float _iFrameDuration = 0.25f;
    [SerializeField] private Color _invulnColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Cooldown")]
    [SerializeField] private float _cooldown = 1f;

    [Header("Input")]
    [SerializeField] private InputActionReference _dashAction;

    private CharacterController _controller;
    private Health _health;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;
    private readonly int _baseColorID = Shader.PropertyToID("_BaseColor");

    private bool _isDashing;
    private float _lastDashTime = -999f;
    private Vector3 _dashDirection;

    public bool IsDashing => _isDashing;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _health = GetComponent<Health>();

        _renderers = GetComponentsInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void OnEnable() => _dashAction.action.Enable();
    private void OnDisable() => _dashAction.action.Disable();

    private void Update()
    {
        if (_dashAction.action.WasPressedThisFrame() && CanDash())
            StartCoroutine(DoDash());
    }

    private bool CanDash() => !_isDashing && Time.time >= _lastDashTime + _cooldown;

    private IEnumerator DoDash()
    {
        _isDashing = true;
        _lastDashTime = Time.time;

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _dashDirection = input.sqrMagnitude > 0.001f
            ? input.ToIso().normalized
            : transform.forward;

        StartCoroutine(IFrameWindow());

        float elapsed = 0f;
        while (elapsed < _dashDuration)
        {
            _controller.Move(_dashDirection * _dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isDashing = false;
    }

    private IEnumerator IFrameWindow()
    {
        if (_health != null) _health.SetInvulnerable(true);

        SetInvulnVisual(true);

        yield return new WaitForSeconds(_iFrameDuration);

        if (_health != null) _health.SetInvulnerable(false);

        SetInvulnVisual(false);
    }

    private void SetInvulnVisual(bool state)
    {
        foreach (var rend in _renderers)
        {
            rend.GetPropertyBlock(_propBlock);

            if (state)
                _propBlock.SetColor(_baseColorID, _invulnColor);
            else
                _propBlock.Clear();

            rend.SetPropertyBlock(_propBlock);
        }
    }
}