using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement movement; // 이동 스크립트 참조

    // 애니메이션 파라미터 이름을 미리 해싱 (성능 최적화)
    private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");
    private readonly int isGroundedHash = Animator.StringToHash("isGrounded");
    private readonly int yVelocityHash = Animator.StringToHash("yVelocity");

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // 1. 달리기 애니메이션: 현재 수평 속도의 절대값을 전달
        float speed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat(moveSpeedHash, speed);

        // 2. 점프/공중 상태 애니메이션: PlayerMovement의 isGrounded를 가져옴
        // (주의: PlayerMovement의 isGrounded 변수가 public이어야 합니다)
        anim.SetBool(isGroundedHash, movement.isGrounded);

        // 3. 상승/하강 애니메이션: Y축 속도를 전달 (상승 시 > 0, 하강 시 < 0)
        anim.SetFloat(yVelocityHash, rb.linearVelocity.y);
    }
}