using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Place in a scene that has a URP Volume for high contrast. Assign the Volume (with Color Adjustments override).
/// Registers it with AccessibilitySettings so the global toggle affects this scene.
/// </summary>
public class HighContrastVolumeBridge : MonoBehaviour
{
    [SerializeField] private Volume _highContrastVolume;

    private void Start()
    {
        if (AccessibilitySettings.Instance != null && _highContrastVolume != null)
            AccessibilitySettings.Instance.SetHighContrastVolume(_highContrastVolume);
    }
}
