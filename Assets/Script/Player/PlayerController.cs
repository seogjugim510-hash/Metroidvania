using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float maxSwingSpeed = 10f;

    [Header("물리 체크 거리")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("공격 설정")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.3f;
    private float lastAttackTime = 0f;

    [Header("대시 및 상태 설정")]
    private bool canDashInAir = true; // 공중 대시 가능 여부
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsWallSliding { get; private set; }
    public int JumpCount { get; private set; }
    public bool IsAttacking { get; private set; }

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private float horizontalInput;
    private GrapplingHook grapplingHook;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        grapplingHook = GetComponent<GrapplingHook>();
    }

    void Update()
    {
        // 공격 중이거나 대시 중이면 입력 및 방향 전환 차단
        if (IsAttacking || IsDashing) return;

        // 에너미를 당기는 중일 때 입력 차단
        if (grapplingHook != null && grapplingHook.isPullingEnemy)
        {
            horizontalInput = 0;
            return;
        }

        // --- 1. 물리 체크 (상태 판정) ---
        Vector2 boxCheckSize = new Vector2(coll.size.x * 0.8f, 0.05f);
        IsGrounded = Physics2D.BoxCast(coll.bounds.center, boxCheckSize, 0f, Vector2.down, coll.bounds.extents.y + groundCheckDistance, groundLayer | wallLayer);

        Vector2 wallDir = new Vector2(horizontalInput != 0 ? Mathf.Sign(horizontalInput) : transform.localScale.x, 0);
        bool isTouchingWall = Physics2D.Raycast(coll.bounds.center, wallDir, wallCheckDistance, wallLayer);

        // 벽 슬라이딩 판정: 벽에 붙어있고, 공중이며, 내려가는 중이고, 벽 쪽으로 입력할 때
        IsWallSliding = (isTouchingWall && !IsGrounded && rb.linearVelocity.y < 0 && horizontalInput != 0);

        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackRoutine()); // 코루틴으로 변경하여 상태 제어
        }

        // --- 2. 대시 충전 로직 (땅 또는 벽 슬라이딩) ---
        if ((IsGrounded && rb.linearVelocity.y <= 0.1f) || IsWallSliding)
        {
            canDashInAir = true;
        }

        // --- 3. 점프 카운트 리셋 ---
        if (IsGrounded && rb.linearVelocity.y <= 0.01f) JumpCount = 0;
        else if (IsWallSliding) JumpCount = 1; // 벽에 붙으면 점프 1회 소모한 것으로 간주 (벽 점프용)
        else if (!IsGrounded && JumpCount == 0 && rb.linearVelocity.y < 0) JumpCount = 1;

        // --- 4. 입력 감지 ---
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }

        if (Input.GetButtonDown("Jump") && (IsGrounded || IsWallSliding || JumpCount < maxJumpCount))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDashInAir)
        {
            StartCoroutine(DashRoutine());
        }

        ApplyFlip();
    }

    private void ApplyFlip()
    {        
        if (IsDashing || IsAttacking) return;

        if (grapplingHook != null && (grapplingHook.isPullingEnemy || grapplingHook.IsHookActive())) return;

        if (IsWallSliding)
        {
            if (horizontalInput != 0)
                transform.localScale = new Vector3(-Mathf.Sign(horizontalInput), 1, 1);
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);
    }
    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 이동 정지

        lastAttackTime = Time.time;
        GetComponent<PlayerAnimation>().PlayAttack();

        // 애니메이션 길이에 맞춰 대기 (예: 0.4초)
        // 혹은 애니메이션 시스템에서 IsAttacking을 false로 바꾸는 이벤트를 주어도 됩니다.
        yield return new WaitForSeconds(attackCooldown);

        IsAttacking = false;
    }

    private void FixedUpdate()
    {
        if (IsDashing || (grapplingHook != null && grapplingHook.isPullingEnemy)) return;

        if (grapplingHook != null && grapplingHook.isGrappling)
        {
            if (horizontalInput != 0)
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * 1.2f, rb.linearVelocity.y);
            }
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        GetComponent<PlayerAnimation>().PlayAttack();
    }
    public void EndAttack()
    {
        IsAttacking = false;
    }

    // [핵심] 이 함수가 실행될 때 주변의 적과 아이템을 동시에 공격합니다.
    public void DoAttackDamage()
    {
        if (attackPoint == null) return;

        // 공격 범위 내의 모든 콜라이더를 가져옵니다.
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        foreach (Collider2D obj in hitObjects)
        {
            // 1. 적(Enemy)인 경우
            EnemyAllyAI enemy = obj.GetComponent<EnemyAllyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                continue; // 적을 맞췄으면 다음 물체로
            }

            // 2. 블릿 아이템(BulletItem)인 경우
            BulletItem item = obj.GetComponent<BulletItem>();
            if (item != null)
            {
                item.TakeDamage(damage); // 아이템 체력 깎음 (10이 깎여서 파괴됨)
                Debug.Log("실수로 아이템을 베었습니다!");
            }
        }
    }

    // 대시 중 데미지를 줄 때도 동일하게 적용
    public void DoDashDamage()
    {
        DoAttackDamage();
    }

    private void Jump()
    {
        JumpCount++;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // 속도 초기화 후 점프
        float currentJumpForce = jumpForce;
        if (JumpCount == 2) currentJumpForce = jumpForce * 0.8f;
        rb.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator DashRoutine()
    {
        IsDashing = true;

        // 공중 대시 사용 시 소모 (벽 슬라이딩 중이 아닐 때만 소모)
        if (!IsGrounded && !IsWallSliding)
        {
            canDashInAir = false;
        }

        float dashInput = Input.GetAxisRaw("Horizontal");
        float dashDirection = (dashInput != 0) ? dashInput : transform.localScale.x;

        transform.localScale = new Vector3(Mathf.Sign(dashDirection), 1, 1);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * moveSpeed * 2.5f, 0);

        yield return new WaitForSeconds(0.2f);

        rb.gravityScale = originalGravity;
        IsDashing = false;
    }

    private void OnDrawGizmos()
    {
        if (coll == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector2 boxCheckSize = new Vector2(coll.size.x * 0.8f, 0.05f);
        Vector3 checkPos = coll.bounds.center + Vector3.down * (coll.bounds.extents.y + groundCheckDistance);
        Gizmos.DrawWireCube(checkPos, boxCheckSize);
    }
}