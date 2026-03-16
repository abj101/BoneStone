using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Listens for Pause action (Escape / gamepad Start), toggles the pause panel.
/// Resume / Settings (tabbed) / Exit to Main Menu.
/// The shared SettingsTabController is opened with a BackToPause callback so that
/// closing settings automatically returns to the pause panel.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    [Header("Settings")]
    [SerializeField] private SettingsTabController _settingsTabController;

    [Header("Input")]
    [SerializeField] private InputActionReference _pauseAction;

    private bool _isPaused;

    private void OnEnable()
    {
        if (_pauseAction != null)
        {
            _pauseAction.action.Enable();
            _pauseAction.action.performed += OnPausePerformed;
        }
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(Resume);
        if (_settingsButton != null)
            _settingsButton.onClick.AddListener(OpenSettings);
        if (_quitButton != null)
            _quitButton.onClick.AddListener(QuitToMenu);
    }

    private void OnDisable()
    {
        if (_pauseAction != null)
        {
            _pauseAction.action.performed -= OnPausePerformed;
            _pauseAction.action.Disable();
        }
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(Resume);
        if (_settingsButton != null)
            _settingsButton.onClick.RemoveListener(OpenSettings);
        if (_quitButton != null)
            _quitButton.onClick.RemoveListener(QuitToMenu);
    }

    private void Start()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_settingsTabController != null)
            _settingsTabController.Close();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;
        Time.timeScale = 0f;

        // Close settings if it was somehow left open
        if (_settingsTabController != null)
            _settingsTabController.Close();

        if (_pausePanel != null)
            _pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;
        Time.timeScale = 1f;

        if (_settingsTabController != null)
            _settingsTabController.Close();

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OpenSettings()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_settingsTabController != null)
            _settingsTabController.Open(BackToPause);
    }

    private void BackToPause()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(true);
    }

    private void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
