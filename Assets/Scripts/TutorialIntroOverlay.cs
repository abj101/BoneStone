using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates a short tutorial overlay in the Tutorial scene and fades it out
/// when the player presses Space (keyboard) or A (controller).
/// </summary>
[DefaultExecutionOrder(-10000)]
public class TutorialIntroOverlay : MonoBehaviour
{
    private const string TutorialSceneName = "Tutorial";
    private const string IntroText =
        "SURVIVE THE ROOM\n\n" +
        "Defeat all enemies to clear the arena.\n\n" +
        "- Move: WASD or Left Stick\n" +
        "- Attack: Left Click / Right Click or Left Shoulder / Right Shoulder\n" +
        "- Slow Time: Shift or Left Shoulder\n" +
        "- Dash: Right Shoulder or Space\n\n" +
        "Press Space or A to begin";

    private CanvasGroup _overlayGroup;
    private bool _fadingOut;
    private EnemySpawner _enemySpawner;
    private static bool _bootstrapInstalled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBootstrap()
    {
        _bootstrapInstalled = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallBootstrap()
    {
        if (_bootstrapInstalled) return;
        _bootstrapInstalled = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TutorialSceneName) return;

        // Avoid duplicates when re-entering the scene.
        if (FindFirstObjectByType<TutorialIntroOverlay>() != null)
            return;

        var go = new GameObject("TutorialIntroOverlayController");
        go.AddComponent<TutorialIntroOverlay>();
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != TutorialSceneName)
        {
            Destroy(this);
            return;
        }

        _enemySpawner = FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);
        if (_enemySpawner != null)
            _enemySpawner.enabled = false;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != TutorialSceneName) return;
        BuildOverlay();
    }

    private void Update()
    {
        if (_fadingOut || _overlayGroup == null)
            return;

        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool aPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (spacePressed || aPressed)
            StartCoroutine(FadeOutAndDestroy());
    }

    private void BuildOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("TutorialOverlayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        var root = new GameObject("TutorialIntroOverlay", typeof(RectTransform), typeof(CanvasGroup));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        _overlayGroup = root.GetComponent<CanvasGroup>();
        _overlayGroup.alpha = 1f;
        _overlayGroup.interactable = false;
        _overlayGroup.blocksRaycasts = false;

        // Dark backdrop for legibility.
        var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var backdropRect = backdrop.GetComponent<RectTransform>();
        backdropRect.SetParent(rootRect, false);
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        var textGo = new GameObject("IntroText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.SetParent(rootRect, false);
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(1100f, 700f);

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = IntroText;
        tmp.fontSize = 42f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        _fadingOut = true;
        float duration = 0.25f;
        float elapsed = 0f;
        float start = _overlayGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _overlayGroup.alpha = Mathf.Lerp(start, 0f, t);
            yield return null;
        }

        if (_enemySpawner != null)
            _enemySpawner.enabled = true;

        if (_overlayGroup != null)
            Destroy(_overlayGroup.gameObject);
        Destroy(gameObject);
    }
}
