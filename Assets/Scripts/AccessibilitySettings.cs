using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Singleton accessibility settings. Persists high contrast and notifies UI/post-process.
/// </summary>
public class AccessibilitySettings : MonoBehaviour
{
    public static AccessibilitySettings Instance { get; private set; }

    private const string PrefsKeyHighContrast = "HighContrast";

    [Header("High Contrast")]
    [Tooltip("Optional: URP Volume with Color Adjustments override for world high contrast.")]
    [SerializeField] private Volume _highContrastVolume;

    public bool HighContrastEnabled { get; private set; }

    public event Action<bool> OnHighContrastChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        HighContrastEnabled = PlayerPrefs.GetInt(PrefsKeyHighContrast, 0) != 0;
        ApplyHighContrastVolume(HighContrastEnabled);
    }

    public void ToggleHighContrast(bool enabled)
    {
        if (HighContrastEnabled == enabled) return;
        HighContrastEnabled = enabled;
        PlayerPrefs.SetInt(PrefsKeyHighContrast, enabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyHighContrastVolume(enabled);
        OnHighContrastChanged?.Invoke(enabled);
    }

    /// <summary>
    /// Call from a scene-specific component (e.g. HighContrastVolumeBridge) to set the Volume used for world high contrast in this scene.
    /// </summary>
    public void SetHighContrastVolume(Volume volume)
    {
        _highContrastVolume = volume;
        ApplyHighContrastVolume(HighContrastEnabled);
    }

    private void ApplyHighContrastVolume(bool enabled)
    {
        if (_highContrastVolume != null)
            _highContrastVolume.weight = enabled ? 1f : 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
