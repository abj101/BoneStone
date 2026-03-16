using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires a UI Toggle to AccessibilitySettings high contrast. Add to settings panel and assign the toggle.
/// </summary>
public class AccessibilitySettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle _highContrastToggle;

    private void Start()
    {
        if (_highContrastToggle == null) return;
        if (AccessibilitySettings.Instance != null)
        {
            _highContrastToggle.SetIsOnWithoutNotify(AccessibilitySettings.Instance.HighContrastEnabled);
            _highContrastToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (_highContrastToggle != null)
            _highContrastToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool value)
    {
        AccessibilitySettings.Instance?.ToggleHighContrast(value);
    }
}
