using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires a UI Toggle to AccessibilitySettings high contrast. Add to settings panel and assign the toggle.
/// </summary>
public class AccessibilitySettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle _highContrastToggle;

    private void Start()
    {
        EnsureReadableAccessibilityRow();

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

    private void EnsureReadableAccessibilityRow()
    {
        if (_highContrastToggle == null)
            _highContrastToggle = GetComponentInChildren<Toggle>(true);
        if (_highContrastToggle == null)
            return;

        // Ensure row has a visible label
        var parent = _highContrastToggle.transform.parent;
        if (parent != null)
        {
            EnsureRowLayout(parent as RectTransform);

            var label = parent.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                var labelGO = new GameObject("HighContrastLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                var labelRT = labelGO.GetComponent<RectTransform>();
                labelRT.SetParent(parent, false);
                labelRT.SetAsFirstSibling();
                labelRT.sizeDelta = new Vector2(300f, 36f);
                label = labelGO.GetComponent<TextMeshProUGUI>();
            }

            if (label.font == null)
                label.font = TMP_Settings.defaultFontAsset;
            label.text = "HIGH CONTRAST";
            label.fontSize = 30f;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;

            var labelLE = label.GetComponent<LayoutElement>();
            if (labelLE == null) labelLE = label.gameObject.AddComponent<LayoutElement>();
            labelLE.minWidth = 360f;
            labelLE.preferredWidth = 360f;
            labelLE.minHeight = 44f;
            labelLE.preferredHeight = 44f;
            labelLE.flexibleWidth = 0f;
        }

        EnsureToggleVisuals(_highContrastToggle);
    }

    private static void EnsureToggleVisuals(Toggle toggle)
    {
        if (toggle == null) return;

        var toggleRT = toggle.GetComponent<RectTransform>();
        if (toggleRT != null)
            toggleRT.sizeDelta = new Vector2(44f, 44f);

        var toggleLE = toggle.GetComponent<LayoutElement>();
        if (toggleLE == null) toggleLE = toggle.gameObject.AddComponent<LayoutElement>();
        toggleLE.minWidth = 44f;
        toggleLE.preferredWidth = 44f;
        toggleLE.minHeight = 44f;
        toggleLE.preferredHeight = 44f;
        toggleLE.flexibleWidth = 0f;

        Image background = toggle.GetComponent<Image>();
        if (background == null)
            background = toggle.gameObject.AddComponent<Image>();
        background.color = new Color(0.16f, 0.16f, 0.22f, 1f);
        toggle.targetGraphic = background;

        Transform checkmarkT = toggle.transform.Find("Checkmark");
        Image checkmark = checkmarkT != null ? checkmarkT.GetComponent<Image>() : null;
        if (checkmark == null)
        {
            var checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var checkRT = checkGO.GetComponent<RectTransform>();
            checkRT.SetParent(toggle.transform, false);
            checkRT.anchorMin = new Vector2(0.2f, 0.2f);
            checkRT.anchorMax = new Vector2(0.8f, 0.8f);
            checkRT.offsetMin = Vector2.zero;
            checkRT.offsetMax = Vector2.zero;
            checkmark = checkGO.GetComponent<Image>();
        }

        checkmark.color = new Color(0.78f, 0.71f, 0.60f, 1f);
        toggle.graphic = checkmark;
    }

    private static void EnsureRowLayout(RectTransform row)
    {
        if (row == null) return;

        var h = row.GetComponent<HorizontalLayoutGroup>();
        if (h == null)
            h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.spacing = 18f;
        h.padding = new RectOffset(12, 12, 8, 8);

        var le = row.GetComponent<LayoutElement>();
        if (le == null) le = row.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 58f;
        le.preferredHeight = 58f;
        le.flexibleHeight = 0f;
    }
}
