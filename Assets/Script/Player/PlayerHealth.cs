using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public HealthUI healthUI;
    private GameOverManager gameOverManager;
    private bool isDead = false; // 중복 사망 방지

    void Awake()
    {
        currentHealth = maxHealth;
        gameOverManager = Object.FindFirstObjectByType<GameOverManager>();
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // 이미 죽었다면 무시

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;

        // 1. 조작 및 물리 완전 정지
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f; // 회전 정지
            rb.bodyType = RigidbodyType2D.Kinematic; // [추가] 외부 물리 영향 차단
        }

        // 2. 애니메이션 실행
        PlayerAnimation anim = GetComponent<PlayerAnimation>();
        if (anim != null) anim.PlayDie();

        // 3. 1.5초 대기 (이 시간 동안은 사망 애니메이션만 재생됨)
        yield return new WaitForSeconds(1.0f);

        // 4. 게임 오버 UI
        if (GameOverManager.Instance != null) GameOverManager.Instance.OnPlayerDeath();
    }

    public void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.SetHealthDisplay(currentHealth);
        }
    }
}