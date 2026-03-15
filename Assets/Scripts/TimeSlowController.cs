using UnityEngine;
using System.Collections;

public class TimeSlowController : MonoBehaviour
{
    public KeyCode slowKey = KeyCode.LeftShift;
    public float slowAmount = 0.3f;
    public float slowDuration = 2f;
    public float cooldown = 5f;

    private bool canSlow = true;
    private float cooldownTimer = 0f;

    public float CooldownProgress
    {
        get
        {
            if (canSlow) return 0f;
            return cooldownTimer / cooldown;
        }
    }

    void Update()
    {
        if (!canSlow)
        {
            cooldownTimer += Time.unscaledDeltaTime;
            if (cooldownTimer >= cooldown)
            {
                cooldownTimer = 0f;
                canSlow = true;
            }
        }

        if (Input.GetKeyDown(slowKey) && canSlow)
        {
            StartCoroutine(SlowTime());
        }
    }

    private IEnumerator SlowTime()
    {
        canSlow = false;
        cooldownTimer = 0f;
        Time.timeScale = slowAmount;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(slowDuration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}