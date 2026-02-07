using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private HealthSystem _healthSystem;
    [SerializeField] private Text _text;

    private void Update()
    {
        if (_healthSystem != null && _text != null)
            _text.text = _healthSystem.CurrentHealth.ToString();
    }
}
