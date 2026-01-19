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
        Debug.Log("플레이어 사망 애니메이션 재생");

        // 1. 플레이어 조작 및 물리 중지
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f; // 추락 중이라면 멈추게 함
        }

        // 2. 사망 애니메이션 재생 (PlayerAnimation 스크립트에 PlayDie가 있다고 가정)
        PlayerAnimation anim = GetComponent<PlayerAnimation>();
        if (anim != null)
        {
            anim.PlayDie(); // 애니메이션 파라미터 Trigger "Die" 등을 실행
        }

        // 3. 애니메이션이 재생될 시간 동안 대기 (예: 1.5초)
        // Time.timeScale이 0이 되어도 돌아가도록 WaitForSecondsRealtime 사용
        yield return new WaitForSeconds(1.5f);

        // 4. 게임 오버 UI 띄우기
        if (gameOverManager != null)
        {
            gameOverManager.OnPlayerDeath();
        }
    }

    public void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.SetHealthDisplay(currentHealth);
        }
    }
}