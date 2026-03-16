using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Applies high-contrast theme (light text on dark semi-opaque background) when AccessibilitySettings has high contrast enabled.
/// Add to any panel that should respond. Caches current colors on enable and restores when high contrast is off.
/// </summary>
public class HighContrastUITheme : MonoBehaviour
{
    private static readonly Color HighContrastText = Color.white;
    private static readonly Color HighContrastPanel = new Color(0f, 0f, 0f, 0.5f);

    private Image _image;
    private TextMeshProUGUI[] _texts;
    private Color _savedPanelColor;
    private Color[] _savedTextColors;
    private bool _applied;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        _savedTextColors = new Color[_texts.Length];
    }

    private void OnEnable()
    {
        if (_image != null)
            _savedPanelColor = _image.color;
        for (int i = 0; i < _texts.Length; i++)
            _savedTextColors[i] = _texts[i].color;

        if (AccessibilitySettings.Instance != null)
        {
            AccessibilitySettings.Instance.OnHighContrastChanged += ApplyTheme;
            ApplyTheme(AccessibilitySettings.Instance.HighContrastEnabled);
        }
    }

    private void OnDisable()
    {
        if (AccessibilitySettings.Instance != null)
            AccessibilitySettings.Instance.OnHighContrastChanged -= ApplyTheme;
    }

    private void ApplyTheme(bool highContrast)
    {
        _applied = highContrast;
        if (_image != null)
            _image.color = highContrast ? HighContrastPanel : _savedPanelColor;
        for (int i = 0; i < _texts.Length; i++)
            _texts[i].color = highContrast ? HighContrastText : _savedTextColors[i];
    }
}
