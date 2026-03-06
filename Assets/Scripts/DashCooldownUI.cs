using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    [SerializeField] private DashController dashController;
    [SerializeField] private Image cooldownOverlay;

    private void Reset()
    {
        cooldownOverlay = GetComponent<Image>();
    }

    private void Update()
    {
        if (dashController == null || cooldownOverlay == null)
            return;

        cooldownOverlay.fillAmount = dashController.CooldownProgress;
    }
}