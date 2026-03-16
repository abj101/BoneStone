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
        public TextMeshProUGUI keyboardBindingLabel;
        public Button keyboardRebindButton;
        public TextMeshProUGUI controllerBindingLabel;
        public Button controllerRebindButton;

        // Legacy fields kept for backward-compatible inspector data.
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

    private enum BindingColumn
    {
        KeyboardMouse,
        Controller,
        LegacyAny
    }

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
        EnsureContainerLayout();
        EnsureHeaderRow();
        NormalizeExistingRows();
        EnsureRowsExist();
        NormalizeExistingRows();
        RefreshAllBindings();
        if (_resetToDefaultsButton != null)
            _resetToDefaultsButton.onClick.AddListener(OnResetToDefaults);
        if (_rebindRows != null)
        {
            for (int i = 0; i < _rebindRows.Length; i++)
            {
                var row = _rebindRows[i];
                string name = row.actionName;

                if (row.keyboardRebindButton != null)
                    row.keyboardRebindButton.onClick.AddListener(() => StartRebind(name, BindingColumn.KeyboardMouse));
                if (row.controllerRebindButton != null)
                    row.controllerRebindButton.onClick.AddListener(() => StartRebind(name, BindingColumn.Controller));

                // Backward-compatible single-button behavior.
                if (row.rebindButton != null && row.keyboardRebindButton == null && row.controllerRebindButton == null)
                    row.rebindButton.onClick.AddListener(() => StartRebind(name, BindingColumn.LegacyAny));
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
                row.actionLabel.text = GetDisplayName(row.actionName);
            RefreshBindingLabels(row);
        }
    }

    private void RefreshBindingLabels(RebindRow row)
    {
        if (!_actionByName.TryGetValue(row.actionName, out var action)) return;

        if (row.keyboardBindingLabel != null)
            row.keyboardBindingLabel.text = GetBindingDisplayForColumn(action, BindingColumn.KeyboardMouse);
        if (row.controllerBindingLabel != null)
            row.controllerBindingLabel.text = GetBindingDisplayForColumn(action, BindingColumn.Controller);

        if (row.bindingLabel != null)
            row.bindingLabel.text = GetBindingDisplayForColumn(action, BindingColumn.LegacyAny);
    }

    private void StartRebind(string actionName, BindingColumn column)
    {
        if (!_actionByName.TryGetValue(actionName, out var action)) return;
        int bindingIndex = FindBindingIndex(action, column);
        if (bindingIndex < 0) return;

        RebindRow row = GetRow(actionName);
        SetPromptLabel(row, column, "Press input...");

        bool mapWasEnabled = _actionMap != null && _actionMap.enabled;
        bool actionWasEnabled = action.enabled;

        // Required by Input System: action must be disabled during rebind.
        if (actionWasEnabled)
            action.Disable();
        if (mapWasEnabled && _actionMap != null)
            _actionMap.Disable();

        var rebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("MousePosition")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op =>
            {
                if (_actionMap != null && mapWasEnabled)
                    _actionMap.Enable();
                else if (actionWasEnabled)
                    action.Enable();

                RefreshBindingLabels(GetRow(actionName));
                if (InputBindingManager.Instance != null)
                    InputBindingManager.Instance.SaveBindings();
                op.Dispose();
            })
            .OnCancel(op =>
            {
                if (_actionMap != null && mapWasEnabled)
                    _actionMap.Enable();
                else if (actionWasEnabled)
                    action.Enable();

                RefreshBindingLabels(GetRow(actionName));
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

    private void EnsureRowsExist()
    {
        if (_actionMap == null) return;

        if (_rebindRows != null && _rebindRows.Length > 0)
            return;

        var rows = new List<RebindRow>();
        string[] desiredOrder = { "Move", "Look", "RightTrigger", "LeftTrigger", "TimeSlow", "Dash" };

        foreach (string actionName in desiredOrder)
        {
            if (!_actionByName.ContainsKey(actionName))
                continue;
            rows.Add(CreateRuntimeRow(actionName));
        }

        _rebindRows = rows.ToArray();
        EnsureResetButtonExists();
    }

    private RebindRow CreateRuntimeRow(string actionName)
    {
        GameObject rowGO = new GameObject($"{actionName}_Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        var rowRT = rowGO.GetComponent<RectTransform>();
        rowRT.SetParent(transform, false);
        rowRT.anchorMin = new Vector2(0f, 1f);
        rowRT.anchorMax = new Vector2(1f, 1f);
        rowRT.pivot = new Vector2(0.5f, 1f);
        rowRT.sizeDelta = new Vector2(0f, 46f);

        var hLayout = rowGO.GetComponent<HorizontalLayoutGroup>();
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        hLayout.spacing = 12f;
        hLayout.padding = new RectOffset(10, 10, 4, 4);

        var layoutElement = rowGO.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        var actionLabel = CreateTmpLabel("ActionLabel", rowRT, GetDisplayName(actionName), 240f);
        var kbLabel = CreateTmpLabel("KeyboardBindingLabel", rowRT, "Unbound", 300f);
        var kbButton = CreateButton("KeyboardRebindButton", rowRT, "Rebind KB/M", 170f);
        var ctrlLabel = CreateTmpLabel("ControllerBindingLabel", rowRT, "Unbound", 300f);
        var ctrlButton = CreateButton("ControllerRebindButton", rowRT, "Rebind Pad", 170f);

        return new RebindRow
        {
            actionName = actionName,
            actionLabel = actionLabel,
            keyboardBindingLabel = kbLabel,
            keyboardRebindButton = kbButton,
            controllerBindingLabel = ctrlLabel,
            controllerRebindButton = ctrlButton
        };
    }

    private void EnsureResetButtonExists()
    {
        if (_resetToDefaultsButton != null) return;
        _resetToDefaultsButton = CreateButton("ResetToDefaultsButton", transform, "Reset to defaults", 240f);
    }

    private static TextMeshProUGUI CreateTmpLabel(string name, Transform parent, string text, float width)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(width, 36f);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (tmp.font == null)
            tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 26f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;

        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.flexibleWidth = 0f;
        return tmp;
    }

    private static Button CreateButton(string name, Transform parent, string buttonText, float width)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(width, 36f);
        go.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.22f, 1f);
        var button = go.GetComponent<Button>();

        var labelGO = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.SetParent(go.transform, false);
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        var tmp = labelGO.GetComponent<TextMeshProUGUI>();
        tmp.text = buttonText;
        if (tmp.font == null)
            tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 24f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;

        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.flexibleWidth = 0f;

        return button;
    }

    private static int FindFirstRebindableBindingIndex(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].isComposite || action.bindings[i].isPartOfComposite)
                continue;
            return i;
        }
        return action.bindings.Count > 0 ? 0 : -1;
    }

    private static string GetDisplayName(string actionName)
    {
        switch (actionName)
        {
            case "RightTrigger": return "RIGHT ATTACK";
            case "LeftTrigger": return "LEFT ATTACK";
            case "TimeSlow": return "SLOW";
            default: return actionName.ToUpperInvariant();
        }
    }

    private void EnsureContainerLayout()
    {
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            // Keep controls table centered and high enough to never cover BACK button.
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -165f);
            rt.sizeDelta = new Vector2(1280f, 520f);
        }

        var vLayout = GetComponent<VerticalLayoutGroup>();
        if (vLayout == null)
            vLayout = gameObject.AddComponent<VerticalLayoutGroup>();

        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandWidth = false;
        vLayout.childForceExpandHeight = false;
        vLayout.spacing = 8f;
        vLayout.padding = new RectOffset(8, 8, 8, 8);
        vLayout.childAlignment = TextAnchor.UpperCenter;

        var fitter = GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Destroy(fitter);
    }

    private void NormalizeExistingRows()
    {
        if (_rebindRows == null || _rebindRows.Length == 0)
            return;

        foreach (var row in _rebindRows)
        {
            EnsureDualColumnFields(row);

            if (row.actionLabel != null)
                NormalizeLabel(row.actionLabel, 240f, 28f, TextAlignmentOptions.MidlineLeft);

            if (row.keyboardBindingLabel != null)
                NormalizeLabel(row.keyboardBindingLabel, 300f, 26f, TextAlignmentOptions.MidlineLeft);
            if (row.controllerBindingLabel != null)
                NormalizeLabel(row.controllerBindingLabel, 300f, 26f, TextAlignmentOptions.MidlineLeft);

            if (row.keyboardRebindButton != null)
                NormalizeButton(row.keyboardRebindButton, 170f);
            if (row.controllerRebindButton != null)
                NormalizeButton(row.controllerRebindButton, 170f);

            if (row.bindingLabel != null)
                NormalizeLabel(row.bindingLabel, 300f, 26f, TextAlignmentOptions.MidlineLeft);
            if (row.rebindButton != null)
                NormalizeButton(row.rebindButton, 170f);

            if (row.actionLabel != null)
            {
                var rowRoot = row.actionLabel.transform.parent as RectTransform;
                if (rowRoot != null)
                    NormalizeRowRoot(rowRoot);
            }
        }

        if (_resetToDefaultsButton != null)
            NormalizeButton(_resetToDefaultsButton, 300f);
    }

    private static void NormalizeLabel(TextMeshProUGUI label, float width, float fontSize, TextAlignmentOptions alignment)
    {
        if (label.font == null)
            label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;

        var rt = label.rectTransform;
        rt.sizeDelta = new Vector2(width, 42f);

        var le = label.GetComponent<LayoutElement>();
        if (le == null) le = label.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 42f;
        le.preferredHeight = 42f;
        le.flexibleWidth = 0f;
        le.flexibleHeight = 0f;
    }

    private static void NormalizeButton(Button button, float width)
    {
        var rt = button.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, 42f);

        var le = button.GetComponent<LayoutElement>();
        if (le == null) le = button.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 42f;
        le.preferredHeight = 42f;
        le.flexibleWidth = 0f;
        le.flexibleHeight = 0f;

        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = new Color(0.16f, 0.16f, 0.22f, 1f);

        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            if (text.font == null) text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 24f;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.alignment = TextAlignmentOptions.Center;
        }
    }

    private static void NormalizeRowRoot(RectTransform rowRoot)
    {
        var h = rowRoot.GetComponent<HorizontalLayoutGroup>();
        if (h == null)
            h = rowRoot.gameObject.AddComponent<HorizontalLayoutGroup>();

        rowRoot.anchorMin = new Vector2(0f, 1f);
        rowRoot.anchorMax = new Vector2(1f, 1f);
        rowRoot.pivot = new Vector2(0.5f, 1f);
        rowRoot.anchoredPosition = Vector2.zero;
        rowRoot.sizeDelta = new Vector2(1240f, 58f);

        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.spacing = 16f;
        h.padding = new RectOffset(12, 12, 6, 6);

        var le = rowRoot.GetComponent<LayoutElement>();
        if (le == null) le = rowRoot.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 54f;
        le.preferredHeight = 58f;
        le.flexibleHeight = 0f;
        le.minWidth = 1240f;
        le.preferredWidth = 1240f;
        le.flexibleWidth = 0f;
    }

    private static void SetPromptLabel(RebindRow row, BindingColumn column, string prompt)
    {
        if (row.actionLabel == null) return;

        switch (column)
        {
            case BindingColumn.KeyboardMouse:
                if (row.keyboardBindingLabel != null) row.keyboardBindingLabel.text = prompt;
                else if (row.bindingLabel != null) row.bindingLabel.text = prompt;
                break;
            case BindingColumn.Controller:
                if (row.controllerBindingLabel != null) row.controllerBindingLabel.text = prompt;
                else if (row.bindingLabel != null) row.bindingLabel.text = prompt;
                break;
            default:
                if (row.bindingLabel != null) row.bindingLabel.text = prompt;
                break;
        }
    }

    private int FindBindingIndex(InputAction action, BindingColumn column)
    {
        if (column == BindingColumn.LegacyAny)
            return FindFirstRebindableBindingIndex(action);

        int idx = FindBindingIndexByScheme(action, column, includeCompositeParts: false);
        if (idx >= 0) return idx;

        // Fall back to parts for composite bindings (e.g. WASD).
        idx = FindBindingIndexByScheme(action, column, includeCompositeParts: true);
        return idx;
    }

    private static int FindBindingIndexByScheme(InputAction action, BindingColumn column, bool includeCompositeParts)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (b.isComposite) continue;
            if (!includeCompositeParts && b.isPartOfComposite) continue;
            if (MatchesColumn(b, column))
                return i;
        }
        return -1;
    }

    private static bool MatchesColumn(InputBinding binding, BindingColumn column)
    {
        string groups = binding.groups ?? string.Empty;
        string path = binding.effectivePath ?? binding.path ?? string.Empty;

        if (column == BindingColumn.KeyboardMouse)
            return groups.Contains("Keyboard&Mouse") || path.Contains("<Keyboard>") || path.Contains("<Mouse>");

        if (column == BindingColumn.Controller)
            return groups.Contains("Gamepad") || path.Contains("<Gamepad>") || path.Contains("XInputController");

        return true;
    }

    private string GetBindingDisplayForColumn(InputAction action, BindingColumn column)
    {
        int idx = FindBindingIndex(action, column);
        if (idx < 0) return "Unbound";

        var b = action.bindings[idx];
        if (b.isPartOfComposite)
        {
            int root = idx - 1;
            while (root >= 0 && action.bindings[root].isPartOfComposite) root--;
            if (root >= 0 && action.bindings[root].isComposite)
            {
                var parts = new List<string>();
                for (int i = root + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
                {
                    if (!MatchesColumn(action.bindings[i], column)) continue;
                    parts.Add(action.GetBindingDisplayString(i));
                }
                if (parts.Count > 0) return string.Join("/", parts);
            }
        }

        return action.GetBindingDisplayString(idx);
    }

    private void EnsureDualColumnFields(RebindRow row)
    {
        if (row == null || row.actionLabel == null) return;
        var rowRoot = row.actionLabel.transform.parent as RectTransform;
        if (rowRoot == null) return;

        if (row.keyboardBindingLabel == null && row.bindingLabel != null)
            row.keyboardBindingLabel = row.bindingLabel;
        if (row.keyboardRebindButton == null && row.rebindButton != null)
            row.keyboardRebindButton = row.rebindButton;

        if (row.keyboardBindingLabel == null)
            row.keyboardBindingLabel = CreateTmpLabel("KeyboardBindingLabel", rowRoot, "Unbound", 300f);
        if (row.keyboardRebindButton == null)
            row.keyboardRebindButton = CreateButton("KeyboardRebindButton", rowRoot, "Rebind KB/M", 170f);

        if (row.controllerBindingLabel == null)
            row.controllerBindingLabel = CreateTmpLabel("ControllerBindingLabel", rowRoot, "Unbound", 300f);
        if (row.controllerRebindButton == null)
            row.controllerRebindButton = CreateButton("ControllerRebindButton", rowRoot, "Rebind Pad", 170f);
    }

    private void EnsureHeaderRow()
    {
        if (transform.Find("RebindHeaderRow") != null) return;

        var header = new GameObject("RebindHeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        var rt = header.GetComponent<RectTransform>();
        rt.SetParent(transform, false);
        rt.SetAsFirstSibling();

        var h = header.GetComponent<HorizontalLayoutGroup>();
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.spacing = 16f;
        h.padding = new RectOffset(12, 12, 6, 6);

        var le = header.GetComponent<LayoutElement>();
        le.minHeight = 42f;
        le.preferredHeight = 42f;
        le.minWidth = 1240f;
        le.preferredWidth = 1240f;

        var action = CreateTmpLabel("ActionHeader", rt, "ACTION", 240f);
        action.fontSize = 24f;
        action.color = new Color(0.78f, 0.71f, 0.60f, 1f);

        var kb = CreateTmpLabel("KeyboardHeader", rt, "KEYBOARD / MOUSE", 470f);
        kb.fontSize = 24f;
        kb.color = new Color(0.78f, 0.71f, 0.60f, 1f);

        var ctrl = CreateTmpLabel("ControllerHeader", rt, "CONTROLLER", 470f);
        ctrl.fontSize = 24f;
        ctrl.color = new Color(0.78f, 0.71f, 0.60f, 1f);
    }
}
