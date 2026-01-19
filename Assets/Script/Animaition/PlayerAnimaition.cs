using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerController move;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (move == null) return;

        bool wallSliding = move.IsWallSliding;

        // 1. 애니메이터 파라미터 업데이트
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsWallSliding", wallSliding);

        // 2. 대시 상태 (여기서 move.IsDashing 대문자 확인)
        anim.SetBool("IsDashing", move.IsDashing);

        // 3. 점프 및 더블 점프 로직
        if (wallSliding)
        {
            anim.SetBool("IsJumping", false);
            anim.SetBool("IsJumping", false);
        }
        else
        {
            anim.SetBool("IsJumping", !move.IsGrounded);
            anim.SetBool("IsJumping", move.JumpCount >= 2);
        }
    }
    public void PlayAttack()
    {
        // 공격 시 다른 공중 모션을 일시적으로 끄거나 우선순위를 높임
        anim.SetBool("IsAttacking", true);

        // 공격 중에는 점프/더블점프 파라미터를 잠시 꺼서 공격 애니메이션이 나오게 유도
        anim.SetBool("IsJumping", false);
        anim.SetBool("IsJumping", false);

        CancelInvoke("StopAttack"); // 이전 Invoke가 있다면 취소
        Invoke("StopAttack", 0.3f);
    }

    private void StopAttack()
    {
        anim.SetBool("IsAttacking", false);
    }
}