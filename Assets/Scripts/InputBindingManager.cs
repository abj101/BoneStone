using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Loads saved input binding overrides on start and provides SaveBindings for rebind UI.
/// Assign the same InputActionAsset used by the player (e.g. InputSystem_Actions).
/// </summary>
public class InputBindingManager : MonoBehaviour
{
    public static InputBindingManager Instance { get; private set; }

    private const string PrefsKeyRebinds = "rebinds";

    [SerializeField] private InputActionAsset _actions;

    public InputActionAsset Actions => _actions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Instance = this;
        else
            Instance = this;
        LoadBindings();
    }

    public void LoadBindings()
    {
        if (_actions == null) return;
        string json = PlayerPrefs.GetString(PrefsKeyRebinds, "");
        if (string.IsNullOrEmpty(json)) return;
        _actions.LoadBindingOverridesFromJson(json);
    }

    public void SaveBindings()
    {
        if (_actions == null) return;
        PlayerPrefs.SetString(PrefsKeyRebinds, _actions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        if (_actions == null) return;
        _actions.RemoveAllBindingOverrides();
        SaveBindings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
