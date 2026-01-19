using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100; // 기본 체력 30 (3칸)
    private int currentHealth;

    public HealthUI healthUI; // 연결할 UI 스크립트

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log("플레이어 사망");         
        }
    }

    public void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.SetHealthDisplay(currentHealth);
        }
    }

    public int GetCurrentHealth() => currentHealth;
}