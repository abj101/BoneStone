using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsIntroUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject controlsIntroPanel;

    [Header("Player Scripts To Disable")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableAtStart;

    private bool introShowing = true;

    private void Start()
    {
        if (controlsIntroPanel != null)
            controlsIntroPanel.SetActive(true);

        SetGameplayEnabled(false);
    }

    private void Update()
    {
        if (!introShowing)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) || (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame))
        {
            CloseIntro();
        }
    }

    public void CloseIntro()
    {
        introShowing = false;

        if (controlsIntroPanel != null)
            controlsIntroPanel.SetActive(false);

        SetGameplayEnabled(true);
    }

    private void SetGameplayEnabled(bool enabled)
    {
        foreach (MonoBehaviour script in scriptsToDisableAtStart)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }
}