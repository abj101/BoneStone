using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool lockY = true;

    private Vector3 _offset;
    private Vector3 _currentVelocity;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned!");
            enabled = false;
            return;
        }

        _offset = transform.position - target.position;
    }

    private void FixedUpdate() // 👈 changed
    {
        Vector3 targetPosition = target.position + _offset;

        if (lockY)
            targetPosition.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime // 👈 important
        );
    }
}
