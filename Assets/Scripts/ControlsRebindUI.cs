using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Rebinding UI: shows current bindings and Rebind buttons. Assign InputActionAsset (or leave null to use InputBindingManager.Instance).
/// Option A: Assign rebindRows with action name, binding text, rebind button per action.
/// Option B: Assign container and rowPrefab to build rows at runtime.
/// </summary>
public class ControlsRebindUI : MonoBehaviour
{
    [System.Serializable]
    public class RebindRow
    {
        public string actionName;
        public TextMeshProUGUI actionLabel;
        public TextMeshProUGUI bindingLabel;
        public Button rebindButton;
    }

    [Header("Input")]
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private string _actionMapName = "Player";

    [Header("Rows (Option A: assign manually)")]
    [SerializeField] private RebindRow[] _rebindRows;

    [Header("Reset")]
    [SerializeField] private Button _resetToDefaultsButton;

    private InputActionMap _actionMap;
    private readonly Dictionary<string, InputAction> _actionByName = new Dictionary<string, InputAction>();

    private void Awake()
    {
        InputActionAsset asset = _actions != null ? _actions : (InputBindingManager.Instance != null ? InputBindingManager.Instance.Actions : null);
        if (asset != null)
        {
            _actions = asset;
            _actionMap = asset.FindActionMap(_actionMapName);
            if (_actionMap != null)
            {
                _actionByName.Clear();
                foreach (var a in _actionMap.actions)
                    _actionByName[a.name] = a;
            }
        }
    }

    private void OnEnable()
    {
        RefreshAllBindings();
        if (_resetToDefaultsButton != null)
            _resetToDefaultsButton.onClick.AddListener(OnResetToDefaults);
        if (_rebindRows != null)
        {
            for (int i = 0; i < _rebindRows.Length; i++)
            {
                var row = _rebindRows[i];
                if (row.rebindButton != null)
                {
                    string name = row.actionName;
                    row.rebindButton.onClick.AddListener(() => StartRebind(name));
                }
            }
        }
    }

    private void OnDisable()
    {
        if (_resetToDefaultsButton != null)
            _resetToDefaultsButton.onClick.RemoveListener(OnResetToDefaults);
        if (_rebindRows != null)
        {
            for (int i = 0; i < _rebindRows.Length; i++)
            {
                var row = _rebindRows[i];
                if (row.rebindButton != null)
                    row.rebindButton.onClick.RemoveAllListeners();
            }
        }
    }

    public void RefreshAllBindings()
    {
        if (_rebindRows == null) return;
        foreach (var row in _rebindRows)
        {
            if (row.actionLabel != null && !string.IsNullOrEmpty(row.actionName))
                row.actionLabel.text = row.actionName;
            RefreshBindingLabel(row);
        }
    }

    private void RefreshBindingLabel(RebindRow row)
    {
        if (row.bindingLabel == null || !_actionByName.TryGetValue(row.actionName, out var action)) return;
        int bindingIndex = 0;
        if (action.bindings.Count > 0)
            row.bindingLabel.text = action.GetBindingDisplayString(bindingIndex);
    }

    private void StartRebind(string actionName)
    {
        if (!_actionByName.TryGetValue(actionName, out var action)) return;
        int bindingIndex = 0;
        if (action.bindings.Count == 0) return;

        RebindRow row = GetRow(actionName);
        if (row.bindingLabel != null)
            row.bindingLabel.text = "Press key...";

        var rebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("MousePosition")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op =>
            {
                RefreshBindingLabel(GetRow(actionName));
                if (InputBindingManager.Instance != null)
                    InputBindingManager.Instance.SaveBindings();
                op.Dispose();
            })
            .OnCancel(op =>
            {
                RefreshBindingLabel(GetRow(actionName));
                op.Dispose();
            });

        rebind.Start();
    }

    private RebindRow GetRow(string actionName)
    {
        if (_rebindRows == null) return default;
        foreach (var row in _rebindRows)
            if (row.actionName == actionName) return row;
        return default;
    }

    private void OnResetToDefaults()
    {
        if (InputBindingManager.Instance != null)
            InputBindingManager.Instance.ResetToDefaults();
        RefreshAllBindings();
    }
}
