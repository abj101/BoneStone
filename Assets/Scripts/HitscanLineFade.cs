using UnityEngine;

public class HitscanLineFade : MonoBehaviour
{
    private LineRenderer _lr;
    private float _duration;
    private float _elapsed;
    private Color _startColor;

    public void Initialize(LineRenderer lr, float duration, Color startColor)
    {
        _lr = lr;
        _duration = Mathf.Max(0.01f, duration);
        _startColor = startColor;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = _elapsed / _duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        Color faded = _startColor;
        faded.a = _startColor.a * (1f - t);
        _lr.startColor = faded;
        _lr.endColor = faded;
    }
}
