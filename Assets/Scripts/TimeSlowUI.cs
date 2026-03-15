using UnityEngine;
using UnityEngine.UI;

public class TimeSlowCooldownUI : MonoBehaviour
{
    [SerializeField] private TimeSlowController timeSlowController;
    [SerializeField] private Image cooldownOverlay;

    private void Reset()
    {
        cooldownOverlay = GetComponent<Image>();
    }

    private void Update()
    {
        if (timeSlowController == null || cooldownOverlay == null)
            return;

        cooldownOverlay.fillAmount = timeSlowController.CooldownProgress;
    }
}