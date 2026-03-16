using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References (auto-found if left empty)")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private EnemySpawner waveManager;

    [Header("End Screen UI (auto-created if left empty)")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI resultLabel;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button mainMenuButton;

    private bool _gameOver;

    void Start()
    {
        Time.timeScale = 1f;

        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerHealth = playerObj.GetComponent<Health>();
        }

        if (waveManager == null)
            waveManager = Object.FindFirstObjectByType<EnemySpawner>();

        if (endScreenPanel == null)
            BuildEndScreenUI();

        endScreenPanel.SetActive(false);

        if (playerHealth != null)
        {
            playerHealth.destroyOnDeath = false;
            playerHealth.OnDeath += OnPlayerDied;
        }

        if (waveManager != null)
            waveManager.OnAllWavesCleared += OnAllEnemiesDefeated;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;

        if (playerHealth != null)
            playerHealth.OnDeath -= OnPlayerDied;

        if (waveManager != null)
            waveManager.OnAllWavesCleared -= OnAllEnemiesDefeated;
    }

    private void OnPlayerDied()
    {
        ShowEndScreen("YOU DIED");
    }

    private void OnAllEnemiesDefeated()
    {
        ShowEndScreen("VICTORY");
    }

    private void ShowEndScreen(string text)
    {
        if (_gameOver) return;
        _gameOver = true;

        if (playerHealth != null)
            DisablePlayerControls(playerHealth.gameObject);

        if (resultLabel != null)
            resultLabel.text = text;

        endScreenPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DisablePlayerControls(GameObject player)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        var ac = player.GetComponent<AttackController>();
        if (ac != null) ac.enabled = false;

        var dc = player.GetComponent<DashController>();
        if (dc != null) dc.enabled = false;
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void BuildEndScreenUI()
    {
        GameObject canvasObj = new GameObject("GameManager_EndScreenCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        endScreenPanel = new GameObject("EndPanel");
        endScreenPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = endScreenPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = endScreenPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.8f);

        resultLabel = CreateTMPLabel(endScreenPanel.transform, "ResultText",
            "", 72, new Vector2(0, 80));

        replayButton = CreateButton(endScreenPanel.transform, "ReplayBtn",
            "RESTART", new Vector2(0, -30), 28);
        replayButton.onClick.AddListener(Replay);

        mainMenuButton = CreateButton(endScreenPanel.transform, "MenuBtn",
            "MAIN MENU", new Vector2(0, -110), 28);
        mainMenuButton.onClick.AddListener(ReturnToMenu);

        endScreenPanel.AddComponent<HighContrastUITheme>();
    }

    private TextMeshProUGUI CreateTMPLabel(Transform parent, string name, string text, float fontSize, Vector2 pos)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 100);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        return tmp;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 pos, float fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 60);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        btn.colors = colors;
        btn.targetGraphic = bg;

        CreateTMPLabel(obj.transform, name + "_Label", label, fontSize, Vector2.zero);

        return btn;
    }
}
