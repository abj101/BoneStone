using UnityEngine;
using System.Collections;

public class BulletBurst : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer tracerPrefab;
    [SerializeField] private LayerMask hitMask;

    [SerializeField] private int bullets = 5;
    [SerializeField] private float spreadAngle = 25f;
    [SerializeField] private float range = 25f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float tracerTime = 0.05f;
    [SerializeField] private float cooldown = 2f;

    private bool _ready = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && _ready)
            StartCoroutine(FireBurst());
    }

    IEnumerator FireBurst()
    {
        _ready = false;

        Vector3 origin = muzzle ? muzzle.position : transform.position;

        float startAngle = -spreadAngle * 0.5f;
        float angleStep = spreadAngle / (bullets - 1);

        for (int i = 0; i < bullets; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            Ray ray = new Ray(origin, dir);
            Vector3 endPoint = origin + dir * range;

            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
            {
                endPoint = hit.point;

                Health health = hit.collider.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(damage);
            }

            LineRenderer tracer = Instantiate(tracerPrefab);
            StartCoroutine(ShowTracer(tracer, origin, endPoint));
        }

        yield return new WaitForSeconds(cooldown);
        _ready = true;
    }

    IEnumerator ShowTracer(LineRenderer tracer, Vector3 start, Vector3 end)
    {
        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);

        yield return new WaitForSeconds(tracerTime);

        Destroy(tracer.gameObject);
    }
}