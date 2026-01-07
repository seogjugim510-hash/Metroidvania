using UnityEngine;

public class PlrayerMove : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 7f; // 가속도
    [SerializeField] private float decceleration = 7f; // 감속도
    [SerializeField] private float velPower = 0.9f; // 마찰력 계산용

    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallMultiplier = 2.5f; // 하강 시 중력 배율
    [SerializeField] private float lowJumpMultiplier = 2f; // 짧게 점프 시 배율

    [Header("지면 체크")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    public bool isGrounded { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 입력 받기
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. 점프 입력
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // 3. 지면 체크
        isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer);

        // 캐릭터 방향 전환
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
    }

    private void FixedUpdate()
    {
        Move();
        BetterJump();
    }

    private void Move()
    {
        // 목표 속도 계산
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;

        // 가속/감속 적용
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // 속도 초기화 후 점프
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void BetterJump()
    {
        // 아래로 떨어질 때 더 빨리 떨어지게 함 (묵직한 느낌)
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // 점프 버튼을 뗐을 때 상승 중이면 속도를 줄임 (가변 점프)
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // 에디터에서 지면 체크 범위를 시각적으로 확인
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
    }
}