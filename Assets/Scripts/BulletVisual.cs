using UnityEngine;

public class BulletVisual : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 2f;

    private Vector3 _target;
    private bool _hasTarget;
    private float _spawnTime;

    public void Initialize(Vector3 target, float overrideSpeed = -1f)
    {
        _target = target;
        _hasTarget = true;
        _spawnTime = Time.time;

        if (overrideSpeed > 0f)
            speed = overrideSpeed;
    }

    private void Awake()
    {
        _spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _spawnTime > lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (!_hasTarget) return;

        transform.position = Vector3.MoveTowards(transform.position, _target, speed * Time.deltaTime);

        if ((transform.position - _target).sqrMagnitude < 0.001f)
            Destroy(gameObject);
    }
}

