using System;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // 대기 및 정찰 상태 제어 (IsWaiting)
    public void SetWaiting(bool isWaiting)
    {
        anim.SetBool("IsWaiting", isWaiting);
    }

    // 공격 상태 제어 (Attack)
    public void SetAttacking(bool isAttacking)
    {
        anim.SetBool("Attack", isAttacking);
    }

    // 피격 애니메이션 실행 (Hit 트리거)
    public void PlayHit()
    {
        anim.SetTrigger("Hit");
    }

    // 사망 상태 제어 (IsDead)
    public void SetDead(bool isDead)
    {
        anim.SetBool("IsDead", isDead);
    }

    // 대시 상태 제어 (IsDashing)
    public void SetDashing(bool isDashing)
    {
        anim.SetBool("IsDashing", isDashing);
    }

    internal bool IsAttacking()
    {
        throw new NotImplementedException();
    }
}