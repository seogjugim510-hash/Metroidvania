using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float maxSwingSpeed = 15f; // 스윙 시 시원함을 위해 약간 높임

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

    // 외부(GrapplingProjectile)에서 접근 가능한 공격 예약 플래그
    public bool isAttackReserved = false;

    [Header("대시 및 상태 설정")]
    private bool canDashInAir = true;
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
        // --- 0. 그래플링 상태 체크 ---
        bool isGrapplingActive = grapplingHook != null && grapplingHook.isGrappling;
        bool isAnyGrapplingAction = grapplingHook != null && (grapplingHook.isPullingEnemy || grapplingHook.isGrappling);

        // 공격 예약 로직
        if (isAnyGrapplingAction && Input.GetKeyDown(KeyCode.Mouse0))
        {
            isAttackReserved = true;
            Debug.Log("공격 예약됨!");
        }

        // 공격 중이거나 대시 중이면 일반 이동 입력 차단
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

        // [핵심 수정] 스윙 중(!isGrapplingActive)이 아닐 때만 벽 슬라이딩 허용
        IsWallSliding = (isTouchingWall && !IsGrounded && rb.linearVelocity.y < 0 && horizontalInput != 0 && !isGrapplingActive);

        // --- 2. 공격 입력 처리 ---
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown)
        {
            // 그래플링(스윙/당기기) 중이 아닐 때만 즉시 공격
            if (!isAnyGrapplingAction)
            {
                StartCoroutine(AttackRoutine());
            }
        }

        // --- 3. 대시 및 점프 로직 ---
        if ((IsGrounded && rb.linearVelocity.y <= 0.1f) || IsWallSliding) canDashInAir = true;

        if (IsGrounded && rb.linearVelocity.y <= 0.01f) JumpCount = 0;
        else if (IsWallSliding) JumpCount = 1;
        else if (!IsGrounded && JumpCount == 0 && rb.linearVelocity.y < 0) JumpCount = 1;

        horizontalInput = Input.GetAxisRaw("Horizontal");

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

    public void ExecuteReservedAttack()
    {
        if (isAttackReserved)
        {
            isAttackReserved = false;
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
        }
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
        // 스윙 관성을 유지하려면 X값 조절 가능, 여기서는 기본 0 고정
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        lastAttackTime = Time.time;
        GetComponent<PlayerAnimation>().PlayAttack();

        yield return new WaitForSeconds(attackCooldown);
        IsAttacking = false;
    }

    private void FixedUpdate()
    {
        if (IsDashing || (grapplingHook != null && grapplingHook.isPullingEnemy)) return;

        // --- 그래플링(스윙) 중 물리 연산 ---
        if (grapplingHook != null && grapplingHook.isGrappling)
        {
            // 스윙 가속 (AddForce로 부드러운 진자 운동 구현)
            if (horizontalInput != 0)
            {
                rb.AddForce(new Vector2(horizontalInput * moveSpeed * 1.5f, 0));
            }

            // 속도 상한선 제한 (Magnitude Clamping)
            if (rb.linearVelocity.magnitude > maxSwingSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSwingSpeed;
            }
            return;
        }

        // 일반 이동
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void EndAttack() => IsAttacking = false;

    public void DoAttackDamage()
    {
        if (attackPoint == null) return;
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {
            EnemyAllyAI enemy = obj.GetComponent<EnemyAllyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                continue;
            }
            BulletItem item = obj.GetComponent<BulletItem>();
            if (item != null) item.TakeDamage(damage);
        }
    }

    public void DoDashDamage() => DoAttackDamage();

    private void Jump()
    {
        JumpCount++;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        float currentJumpForce = JumpCount == 2 ? jumpForce * 0.8f : jumpForce;
        rb.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator DashRoutine()
    {
        IsDashing = true;
        if (!IsGrounded && !IsWallSliding) canDashInAir = false;

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