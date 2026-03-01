using UnityEngine;
using System.Collections;

public class HitscanPistol : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer tracer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private LayerMask hitMask;

    [SerializeField] private float range = 25f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float tracerTime = 0.05f;

    private float _nextFireTime;

    private void Awake()
    {
        if (tracer != null)
        {
            tracer.enabled = false;
            tracer.SetPosition(0, Vector3.zero);
            tracer.SetPosition(1, Vector3.zero);
        }
    }

    private void Update()
    {
        if (Input.GetButton("Fire1"))
            TryShoot();
    }

    private void TryShoot()
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireRate;
        Shoot();
    }

    private void Shoot()
    {
        Vector3 origin = muzzle ? muzzle.position : transform.position;
        Vector3 dir = transform.forward;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        Ray ray = new Ray(origin, dir);
        Vector3 endPoint = origin + dir * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            endPoint = hit.point;
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        if (tracer != null)
            StartCoroutine(ShowTracer(origin, endPoint));
    }

    private IEnumerator ShowTracer(Vector3 start, Vector3 end)
    {
        tracer.enabled = true;
        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);
        yield return new WaitForSeconds(tracerTime);
        tracer.enabled = false;
        tracer.SetPosition(0, Vector3.zero);
        tracer.SetPosition(1, Vector3.zero);
    }
}