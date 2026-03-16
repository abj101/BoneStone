using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private SettingsTabController _settingsTabController;

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        if (_mainMenuPanel != null)
            _mainMenuPanel.SetActive(false);

        if (_settingsTabController != null)
            _settingsTabController.Open(BackToMenu);
    }

    private void BackToMenu()
    {
        if (_mainMenuPanel != null)
            _mainMenuPanel.SetActive(true);
    }
}
