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
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("공격 설정")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private int comboStep = 0; 
    [SerializeField] private float comboWindow = 0.5f; 
    private Coroutine comboResetCoroutine;
    private bool comboReserved = false; // 다음 콤보 예약 플래그
    private float lastAttackTime = 0f;

    // 콤보 연계를 위한 변수 추가
    private bool comboPossible = false;

    public bool isAttackReserved = false;

    [Header("대시 및 상태 설정")]
    private bool canDashInAir = true;
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsWallSliding { get; private set; }
    public int JumpCount { get; private set; }
    public bool IsAttack { get; private set; }

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private float horizontalInput;
    private GrapplingHook grapplingHook;
    private float originalScaleX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        grapplingHook = GetComponent<GrapplingHook>();
        originalScaleX = transform.localScale.x;
    }

    void Update()
    {
        // 1. 물리 체크
        Vector2 boxCheckSize = new Vector2(coll.size.x * 5f, 0.05f);
        IsGrounded = Physics2D.BoxCast(coll.bounds.center, boxCheckSize, 0f, Vector2.down, coll.bounds.extents.y + groundCheckDistance, groundLayer | wallLayer);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!IsAttack)
            {
                // 첫 공격 시작
                StartCoroutine(AttackRoutine());
            }
            else if (comboPossible && !comboReserved)
            {
                // 공격 중이고 콤보 가능 타이밍일 때 클릭하면 '예약'
                comboReserved = true;
            }
        }

        // 공격/대시 중 이동 제어 (입력은 위에서 받았으므로 로직만 차단)
        if (IsAttack || IsDashing)
        {
            horizontalInput = 0;
            if (IsGrounded && !IsDashing)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // --- 일반 이동 입력 처리 ---
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (IsGrounded && rb.linearVelocity.y <= 0.01f) JumpCount = 0;
        if (Input.GetButtonDown("Jump") && (IsGrounded || JumpCount < maxJumpCount)) Jump();

        ApplyFlip();
    }

    private IEnumerator AttackRoutine()
    {
        IsAttack = true;
        comboReserved = false;
        comboPossible = false;

        // 콤보 단계 설정
        comboStep++;
        if (comboStep > 2) comboStep = 1;

        // 물리 정지 및 애니메이션 재생
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        lastAttackTime = Time.time;
        GetComponent<PlayerAnimation>().PlayAttack(comboStep);

        // [중요] 애니메이션이 절반 정도 진행되었을 때부터 클릭 입력 허용 (선입력 윈도우)
        float windowStart = attackCooldown * 0.4f;
        yield return new WaitForSeconds(windowStart);
        comboPossible = true;

        // 나머지 쿨타임 대기
        yield return new WaitForSeconds(attackCooldown - windowStart);

        // 만약 쿨타임이 끝났을 때 예약된 클릭이 있다면 즉시 다음 콤보 실행
        if (comboReserved)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            // 예약된 클릭이 없으면 공격 상태 종료
            IsAttack = false;
            comboPossible = false;

            if (comboResetCoroutine != null) StopCoroutine(comboResetCoroutine);
            comboResetCoroutine = StartCoroutine(ResetComboAfterTime());
        }
    }

    private IEnumerator ResetComboAfterTime()
    {
        yield return new WaitForSeconds(comboWindow);
        // 콤보 윈도우가 끝나면 스테이지 초기화 (0으로 돌아감)
        if (!IsAttack) comboStep = 0;
    }

    // --- 이하 기존 함수들 (FixedUpdate, Jump, ApplyFlip 등) 동일 ---
    private void FixedUpdate()
    {
        if (IsAttack || IsDashing || (grapplingHook != null && grapplingHook.isPullingEnemy)) return;

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

    private void Jump()
    {
        JumpCount++;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        float currentJumpForce = JumpCount == 2 ? jumpForce * 0.8f : jumpForce;
        rb.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);
    }
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
        if (IsDashing || IsAttack) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.localScale = new Vector3((mousePos.x > transform.position.x ? 1 : -1) * originalScaleX, originalScaleX, 1);
    }

    private void OnDrawGizmos()
    {
        if (coll == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector3 checkPos = coll.bounds.center + Vector3.down * (coll.bounds.extents.y + groundCheckDistance);
        Gizmos.DrawWireCube(checkPos, new Vector2(coll.size.x * 5f, 0.05f));
        if (attackPoint != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(attackPoint.position, attackRange); }
    }
}