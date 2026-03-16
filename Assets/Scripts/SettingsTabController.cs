using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages a dark tabbed settings panel shared by both the main menu and pause menu.
/// Callers pass an optional onClose callback so the panel never needs to know its context.
///
/// Inspector wiring:
///   _panel          — root SettingsPanel GameObject (set inactive in scene)
///   _tabButtons     — [0] Audio  [1] Controls  [2] Accessibility  (Button components)
///   _tabPanels      — [0] Content_Audio  [1] Content_Controls  [2] Content_Accessibility
///   _tabIndicators  — one thin bottom-border Image per tab (child of each tab button)
///   _closeButton    — the ✕ button in the top-right corner
/// </summary>
public class SettingsTabController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panel;

    [Header("Tabs")]
    [SerializeField] private Button[] _tabButtons;
    [SerializeField] private GameObject[] _tabPanels;
    [SerializeField] private Image[] _tabIndicators;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    // Colors applied at runtime — tweak in the Inspector or leave as-is.
    [Header("Tab Style")]
    [SerializeField] private Color _tabActiveTextColor   = new Color(1f,   1f,   1f,   1f);
    [SerializeField] private Color _tabInactiveTextColor = new Color(0.53f, 0.53f, 0.6f, 1f);
    [SerializeField] private Color _indicatorColor       = new Color(0.78f, 0.71f, 0.60f, 1f);

    private Action _onCloseCallback;
    private int _activeTab = -1;

    private void OnEnable()
    {
        EnsureTabVisuals();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            int captured = i;
            if (_tabButtons[i] != null)
                _tabButtons[i].onClick.AddListener(() => SelectTab(captured));
        }
    }

    private void OnDisable()
    {
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            if (_tabButtons[i] != null)
                _tabButtons[i].onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Shows the settings panel, defaulting to the first tab.
    /// <paramref name="onClose"/> is invoked when Close() is called.
    /// </summary>
    public void Open(Action onClose = null)
    {
        _onCloseCallback = onClose;
        if (_panel != null)
            _panel.SetActive(true);
        SelectTab(0);
    }

    /// <summary>
    /// Hides the panel and invokes the registered onClose callback.
    /// </summary>
    public void Close()
    {
        if (_panel != null)
            _panel.SetActive(false);

        Action callback = _onCloseCallback;
        _onCloseCallback = null;
        callback?.Invoke();
    }

    /// <summary>
    /// Activates the tab content panel at <paramref name="index"/> and updates visual state.
    /// </summary>
    public void SelectTab(int index)
    {
        if (index == _activeTab) return;
        _activeTab = index;

        for (int i = 0; i < _tabPanels.Length; i++)
        {
            bool active = i == index;

            if (i < _tabPanels.Length && _tabPanels[i] != null)
                _tabPanels[i].SetActive(active);

            if (i < _tabIndicators.Length && _tabIndicators[i] != null)
                _tabIndicators[i].gameObject.SetActive(active);

            if (i < _tabButtons.Length && _tabButtons[i] != null)
            {
                var label = _tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.color = active ? _tabActiveTextColor : _tabInactiveTextColor;

                // Tint the button's own graphic so the active tab reads clearly
                var graphic = _tabButtons[i].targetGraphic;
                if (graphic != null)
                    graphic.color = active
                        ? new Color(1f, 1f, 1f, 0.06f)
                        : new Color(1f, 1f, 1f, 0f);
            }
        }

        // Keep indicator color consistent even if it was untinted at authoring time
        if (index < _tabIndicators.Length && _tabIndicators[index] != null)
            _tabIndicators[index].color = _indicatorColor;
    }

    private void EnsureTabVisuals()
    {
        if (_tabButtons == null || _tabButtons.Length == 0)
            return;

        if (_tabIndicators == null || _tabIndicators.Length != _tabButtons.Length)
            _tabIndicators = new Image[_tabButtons.Length];

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            if (_tabButtons[i] == null) continue;

            EnsureTabLabel(_tabButtons[i], i);
            _tabIndicators[i] = EnsureIndicator(_tabButtons[i], _tabIndicators[i]);
        }
    }

    private void EnsureTabLabel(Button tabButton, int index)
    {
        var label = tabButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
        {
            var labelGO = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rt = labelGO.GetComponent<RectTransform>();
            rt.SetParent(tabButton.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            label = labelGO.GetComponent<TextMeshProUGUI>();
        }

        if (string.IsNullOrWhiteSpace(label.text))
            label.text = GetDefaultTabLabel(index);

        if (label.font == null)
            label.font = TMP_Settings.defaultFontAsset;

        label.alignment = TextAlignmentOptions.Center;
    }

    private Image EnsureIndicator(Button tabButton, Image currentIndicator)
    {
        if (currentIndicator != null) return currentIndicator;

        var existing = tabButton.transform.Find("Indicator");
        if (existing != null)
        {
            var existingImage = existing.GetComponent<Image>();
            if (existingImage != null) return existingImage;
        }

        var indicatorGO = new GameObject("Indicator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = indicatorGO.GetComponent<RectTransform>();
        rt.SetParent(tabButton.transform, false);
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, 3f);

        var image = indicatorGO.GetComponent<Image>();
        image.color = _indicatorColor;
        indicatorGO.SetActive(false);
        return image;
    }

    private static string GetDefaultTabLabel(int index)
    {
        switch (index)
        {
            case 0: return "AUDIO";
            case 1: return "CONTROLS";
            case 2: return "ACCESSIBILITY";
            default: return "TAB";
        }
    }
}
