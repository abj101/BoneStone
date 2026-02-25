using UnityEngine;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private TextMeshProUGUI healthText;

    void Start()
    {
        playerHealth.OnHealthChanged += UpdateHealthText;
        UpdateHealthText(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdateHealthText(int current, int max)
    {
        healthText.text = $"Health: {current}/{max}";
    }

    void OnDestroy()
    {
        playerHealth.OnHealthChanged -= UpdateHealthText;
    }
}