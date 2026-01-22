using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerController move;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();
        move = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (move == null || anim == null) return;
        if (move.IsAttack) return;

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        if (!move.IsGrounded && !move.IsWallSliding)
        {
            anim.SetBool("IsJump", true);
            anim.SetBool("IsFall", rb.linearVelocity.y < -0.01f);
        }
        else
        {
            anim.SetBool("IsJump", false);
            anim.SetBool("IsFall", false);
        }
        anim.SetBool("IsWallSliding", move.IsWallSliding);
        anim.SetBool("IsDashing", move.IsDashing);
    }

    public void PlayAttack(int step)
    {
        anim.SetBool("IsJump", false);
        anim.SetBool("IsFall", false);

        // ComboStep을 먼저 설정하고 IsAttack 트리거를 켜야 함
        anim.SetInteger("ComboStep", step);
        anim.SetBool("IsAttack", true);

        CancelInvoke("StopAttack");
        // 콤보가 부드럽게 이어지려면 StopAttack(IsAttack=false)을 쿨타임보다 약간 더 길게 설정
        Invoke("StopAttack", 0.45f);
    }

    private void StopAttack()
    {
        anim.SetBool("IsAttack", false);
        // 여기서 ComboStep을 0으로 만들지 마세요 (콤보 윈도우 유지를 위해)
    }

    public void PlayDie()
    {
        anim.SetBool("IsDead", true);
    }
}