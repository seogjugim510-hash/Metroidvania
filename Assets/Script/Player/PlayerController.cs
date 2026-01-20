using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float maxSwingSpeed = 15f;

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

    // [중요] 스케일 수정을 위한 변수
    private float originalScaleX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        grapplingHook = GetComponent<GrapplingHook>();

        // 시작 시 인스펙터에 설정된 스케일 값을 저장 (예: 2.0 또는 3.0)
        originalScaleX = transform.localScale.x;
    }

    void Update()
    {
        bool isGrapplingActive = grapplingHook != null && grapplingHook.isGrappling;
        bool isAnyGrapplingAction = grapplingHook != null && (grapplingHook.isPullingEnemy || grapplingHook.isGrappling);

        if (isAnyGrapplingAction && Input.GetKeyDown(KeyCode.Mouse0))
        {
            isAttackReserved = true;
        }

        if (IsAttacking || IsDashing) return;

        if (grapplingHook != null && grapplingHook.isPullingEnemy)
        {
            horizontalInput = 0;
            return;
        }

        // --- 물리 체크 ---
        Vector2 boxCheckSize = new Vector2(coll.size.x * 0.8f, 0.05f);
        IsGrounded = Physics2D.BoxCast(coll.bounds.center, boxCheckSize, 0f, Vector2.down, coll.bounds.extents.y + groundCheckDistance, groundLayer | wallLayer);

        Vector2 wallDir = new Vector2(horizontalInput != 0 ? Mathf.Sign(horizontalInput) : transform.localScale.x, 0);
        bool isTouchingWall = Physics2D.Raycast(coll.bounds.center, wallDir, wallCheckDistance, wallLayer);

        // 스윙 중에는 벽 슬라이딩 방지
        IsWallSliding = (isTouchingWall && !IsGrounded && rb.linearVelocity.y < 0 && horizontalInput != 0 && !isGrapplingActive);

        // --- 공격 및 이동 입력 ---
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown)
        {
            if (!isAnyGrapplingAction) StartCoroutine(AttackRoutine());
        }

        if ((IsGrounded && rb.linearVelocity.y <= 0.1f) || IsWallSliding) canDashInAir = true;
        if (IsGrounded && rb.linearVelocity.y <= 0.01f) JumpCount = 0;
        else if (IsWallSliding) JumpCount = 1;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && (IsGrounded || IsWallSliding || JumpCount < maxJumpCount)) Jump();
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDashInAir) StartCoroutine(DashRoutine());

        ApplyFlip();
    }

    public void ExecuteReservedAttack()
    {
        if (isAttackReserved)
        {
            isAttackReserved = false;
            if (Time.time >= lastAttackTime + attackCooldown) StartCoroutine(AttackRoutine());
        }
    }

    private void ApplyFlip()
    {
        if (IsDashing || IsAttacking) return;
        if (grapplingHook != null && (grapplingHook.isPullingEnemy || grapplingHook.IsHookActive())) return;

        // originalScaleX를 사용하여 현재 설정된 크기를 유지하면서 방향만 바꿈
        if (IsWallSliding)
        {
            if (horizontalInput != 0)
                transform.localScale = new Vector3(-Mathf.Sign(horizontalInput) * originalScaleX, originalScaleX, 1);
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > transform.position.x)
            transform.localScale = new Vector3(originalScaleX, originalScaleX, 1);
        else
            transform.localScale = new Vector3(-originalScaleX, originalScaleX, 1);
    }

    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        lastAttackTime = Time.time;
        GetComponent<PlayerAnimation>().PlayAttack();
        yield return new WaitForSeconds(attackCooldown);
        IsAttacking = false;
    }

    private void FixedUpdate()
    {
        if (IsDashing || (grapplingHook != null && grapplingHook.isPullingEnemy)) return;

        if (grapplingHook != null && grapplingHook.isGrappling)
        {
            if (horizontalInput != 0)
                rb.AddForce(new Vector2(horizontalInput * moveSpeed * 1.5f, 0));

            if (rb.linearVelocity.magnitude > maxSwingSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSwingSpeed;
            return;
        }

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
            if (enemy != null) { enemy.TakeDamage(damage); continue; }
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

        // 대시 방향 전환 시에도 스케일 유지
        transform.localScale = new Vector3(Mathf.Sign(dashDirection) * originalScaleX, originalScaleX, 1);

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