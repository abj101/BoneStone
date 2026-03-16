using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private string menuSceneName = "Menu";

    private bool isPaused;

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void Resume()
    {
        if (!isPaused) return;
        TogglePause();
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }
}
