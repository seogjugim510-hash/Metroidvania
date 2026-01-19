using UnityEngine;

public class GrapplingProjectile : MonoBehaviour
{
    public float speed = 30f;
    public float maxDistance = 5f;
    public float pullSpeed = 100f; // 에너미를 끌어당기는 속도

    private Vector2 startPos;
    private Rigidbody2D rb;
    private GrapplingHook owner;
    private bool isHit = false;

    public void Initialize(GrapplingHook hook, Vector2 direction)
    {
        owner = hook;
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        // 1. 이미 무언가에 맞았거나, 아직 설정되지 않았다면 로직 중단
        if (isHit || owner == null) return;

        // 2. 최대 거리 도달 체크
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            isHit = true; // 중복 실행 방지
            owner.ResetGrapple();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHit) return; // 이미 무언가에 맞았다면 무시

        if (collision.CompareTag("BigEnemy"))
        {
            isHit = true; // [중요] 맞자마자 true로 설정해 중복 코루틴 방지
            rb.linearVelocity = Vector2.zero; // 투사체 멈춤
            rb.bodyType = RigidbodyType2D.Static; // 투사체 고정

            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            StopAllCoroutines();
            StartCoroutine(FlyToPoint(hitPoint, collision.transform));
        }
        else if (collision.CompareTag("Enemy"))
        {
            isHit = true;
            StopAllCoroutines();
            StartCoroutine(PullEnemy(collision.transform));
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            isHit = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            owner.OnProjectileHit(hitPoint);
        }
        if (collision.CompareTag("BulletItem"))
        {
            BulletItem item = collision.GetComponent<BulletItem>();
            if (item != null)
            {
                PlayerRangedAttack attack = owner.GetComponent<PlayerRangedAttack>();
                item.StartCollection(attack); // 날아오기 시작!
            }

            // 훅 투사체는 역할을 다했으므로 소멸 및 회수
            owner.ResetGrapple();
            Destroy(gameObject);
        }
    }
    System.Collections.IEnumerator FlyToPoint(Vector2 targetPoint, Transform enemyTransform)
    {
        // 플레이어의 물리 엔진 조작을 위해 참조 가져오기
        Rigidbody2D playerRb = owner.GetComponent<Rigidbody2D>();
        float originalGravity = playerRb.gravityScale;

        // 플레이어 상태 설정 (이동 로직 방해 금지)
        owner.isPullingEnemy = true;
        playerRb.gravityScale = 0f;

        // 플레이어와 적의 충돌 무시
        Collider2D playerCol = owner.GetComponent<Collider2D>();
        Collider2D enemyCol = enemyTransform.GetComponent<Collider2D>();
        if (playerCol != null && enemyCol != null)
            Physics2D.IgnoreCollision(playerCol, enemyCol, true);

        float flySpeed = 30f; // 속도
        float arrivalDistance = 1.0f; // 도착 판정 거리 (너무 짧으면 멈춤 현상 발생)

        // 도착할 때까지 반복
        while (enemyTransform != null && Vector2.Distance(owner.transform.position, targetPoint) > arrivalDistance)
        {
            Vector2 dir = (targetPoint - (Vector2)owner.transform.position).normalized;
            playerRb.linearVelocity = dir * flySpeed;

            // 줄 시각화 위치 고정
            transform.position = targetPoint;
            yield return null;
        }

        // [마무리 로직] 매우 중요!
        playerRb.linearVelocity = Vector2.zero;
        playerRb.gravityScale = originalGravity;

        if (playerCol != null && enemyCol != null)
            Physics2D.IgnoreCollision(playerCol, enemyCol, false);

        // 모든 상태 초기화 후 삭제
        owner.isPullingEnemy = false;
        owner.ResetGrapple();
        Destroy(gameObject);
    }
    System.Collections.IEnumerator PullEnemy(Transform enemyTransform)
    {
        Rigidbody2D enemyRb = enemyTransform.GetComponent<Rigidbody2D>();
        EnemyAllyAI enemyAI = enemyTransform.GetComponent<EnemyAllyAI>();

        // 플레이어와 에너미의 콜라이더 가져오기
        Collider2D enemyCol = enemyTransform.GetComponent<Collider2D>();
        Collider2D playerCol = owner.GetComponent<Collider2D>();

        owner.isPullingEnemy = true;
        float fixedFacingDir = Mathf.Sign(owner.transform.localScale.x);
        float stopDistance = 1.5f;

        if (enemyAI != null) enemyAI.SetStun(true);

        // [핵심] 당기기 시작할 때 플레이어와 이 에너미 사이의 충돌을 무시 설정
        if (enemyCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, enemyCol, true);
        }

        while (enemyTransform != null)
        {
            Vector2 targetPos = (Vector2)owner.transform.position + new Vector2(fixedFacingDir * stopDistance, 0);
            float distanceToTarget = Vector2.Distance(targetPos, enemyTransform.position);

            if (distanceToTarget < 0.2f) break;

            Vector2 pullDirection = (targetPos - (Vector2)enemyTransform.position).normalized;
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = pullDirection * pullSpeed;
            }

            transform.position = enemyTransform.position;
            yield return null;
        }

        // 도착 후 처리
        if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;

        // [핵심] 당기기가 끝난 후 다시 충돌이 발생하도록 설정 복구
        if (enemyCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, enemyCol, false);
        }

        if (enemyAI != null) enemyAI.SetStun(false);

        owner.ResetGrapple();
        Destroy(gameObject);
    }
}