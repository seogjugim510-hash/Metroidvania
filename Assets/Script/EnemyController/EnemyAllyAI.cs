using UnityEngine;

public class EnemyAllyAI : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    private EnemyAnimation enemyAnim;
    private Rigidbody2D rb;
    private Transform player;

    [Header("정찰 설정")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    private float leftBoundary;
    private float rightBoundary;
    private bool movingRight = true;

    [Header("추격 및 공격 설정")]
    public float chaseSpeed = 4f;
    public float detectionRange = 6f;
    public float attackRange = 0.9f;

    [Tooltip("다음 공격까지 걸리는 대기 시간(초)")]
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime;

    public Transform attackPoint;
    public LayerMask playerLayer;
    public int damage = 10;

    [Header("능력치")]
    public int health = 20;
    private bool isDead = false;

    [Header("상태 확인")]
    public bool isStunned = false;
    public bool isAttackingNow = false; // 공격 중인지 여부

    public GameClearManager gameClearManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyAnim = GetComponent<EnemyAnimation>();

        leftBoundary = transform.position.x - patrolDistance;
        rightBoundary = transform.position.x + patrolDistance;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // 사망, 플레이어 부재, 스턴, 또는 '공격 중'이면 모든 행동(이동/방향전환) 차단
        if (isDead || player == null || isStunned || isAttackingNow)
        {
            // 공격 중이거나 스턴 중이면 속도를 0으로 고정
            if (isAttackingNow || isStunned)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                ExecuteAttackState();
            }
            else
            {
                // 공격 쿨타임 중에는 정지
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                enemyAnim.SetAttacking(false);
            }
        }
        else if (distance <= detectionRange)
        {
            ExecuteChaseState();
        }
        else
        {
            ExecutePatrolState();
        }
    }

    void ExecutePatrolState()
    {
        enemyAnim.SetWaiting(false);
        enemyAnim.SetAttacking(false);

        float currentSpeed = movingRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (movingRight && transform.position.x >= rightBoundary) movingRight = false;
        else if (!movingRight && transform.position.x <= leftBoundary) movingRight = true;

        UpdateFacing(movingRight ? 1 : -1);
    }

    void ExecuteChaseState()
    {
        enemyAnim.SetWaiting(false);
        enemyAnim.SetAttacking(false);

        float direction = player.position.x > transform.position.x ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        UpdateFacing(direction);
    }

    void ExecuteAttackState()
    {
        isAttackingNow = true; // 공격 상태 돌입 (이동/방향전환 차단 시작)
        rb.linearVelocity = Vector2.zero;
        enemyAnim.SetAttacking(true);
        lastAttackTime = Time.time;
    }

    // [중요] 애니메이션 마지막 프레임에 이 이벤트를 넣으세요.
    public void EndAttack()
    {
        isAttackingNow = false; // 공격 상태 해제
        enemyAnim.SetAttacking(false);
    }

    void UpdateFacing(float horizontal)
    {
        if (horizontal > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        enemyAnim.PlayHit();

        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        isAttackingNow = false;
        enemyAnim.SetDead(true);
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;

        if (gameObject.CompareTag("BigEnemy"))
        {
            if (gameClearManager != null)
            {
                Invoke("TriggerClearUI", 1.5f);
            }
        }
    }

    // 애니메이션 이벤트에서 데미지 판정 시점 호출
    public void DoDashDamage()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

        if (hitPlayer != null)
        {
            PlayerHealth playerHealth = hitPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("플레이어 데미지 입음");
            }
        }
    }

    public void SetStun(bool state)
    {
        isStunned = state;
        if (state)
        {
            isAttackingNow = false; // 스턴 시 공격 강제 취소
            rb.linearVelocity = Vector2.zero;
            enemyAnim.SetAttacking(false);
            enemyAnim.SetWaiting(true);
        }
    }

    void TriggerClearUI()
    {
        gameClearManager.OnGameClear();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}