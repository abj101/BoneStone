using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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
    private VolumeProfile _runtimeProfile;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        HighContrastEnabled = PlayerPrefs.GetInt(PrefsKeyHighContrast, 0) != 0;
        TryBindBestVolumeInScene();
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
        EnsureHighContrastOverrides();
        ApplyHighContrastVolume(HighContrastEnabled);
    }

    private void ApplyHighContrastVolume(bool enabled)
    {
        if (_highContrastVolume == null) return;

        EnsureHighContrastOverrides();
        _highContrastVolume.weight = enabled ? 1f : 0f;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindBestVolumeInScene();
        ApplyHighContrastVolume(HighContrastEnabled);
        OnHighContrastChanged?.Invoke(HighContrastEnabled);
    }

    private void TryBindBestVolumeInScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Volume bestGlobal = null;
        Volume fallbackSameScene = null;
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < volumes.Length; i++)
        {
            Volume v = volumes[i];
            if (v == null) continue;
            if (v.gameObject.scene != activeScene) continue;

            if (fallbackSameScene == null)
                fallbackSameScene = v;
            if (v.isGlobal)
            {
                bestGlobal = v;
                break;
            }
        }

        _highContrastVolume = bestGlobal != null ? bestGlobal : fallbackSameScene;
        _runtimeProfile = null; // Force per-scene profile rebuild when scene volume changes.
    }

    private void EnsureHighContrastOverrides()
    {
        if (_highContrastVolume == null) return;

        if (_runtimeProfile == null)
        {
            if (_highContrastVolume.sharedProfile != null)
                _runtimeProfile = Instantiate(_highContrastVolume.sharedProfile);
            else if (_highContrastVolume.profile != null)
                _runtimeProfile = Instantiate(_highContrastVolume.profile);
            else
                _runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();

            _runtimeProfile.name = "RuntimeHighContrastProfile";
            _highContrastVolume.profile = _runtimeProfile;
        }

        if (!_runtimeProfile.TryGet(out ColorAdjustments color))
            color = _runtimeProfile.Add<ColorAdjustments>(true);

        color.active = true;
        color.contrast.Override(40f);
        color.postExposure.Override(0.2f);
        color.saturation.Override(15f);
        color.colorFilter.Override(Color.white);
    }
}
