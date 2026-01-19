using UnityEngine;
using System.Collections;

public class GrapplingProjectile : MonoBehaviour
{
    public float speed = 30f;
    public float maxDistance = 5f;
    public float pullSpeed = 100f; // 에너미를 끌어당기는 속도

    private Vector2 startPos;
    private Rigidbody2D rb;
    private GrapplingHook owner;
    private bool isHit = false;

    // [추가] 플레이어 컨트롤러 참조를 위한 변수
    private PlayerController playerController;

    public void Initialize(GrapplingHook hook, Vector2 direction)
    {
        owner = hook;
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        rb.linearVelocity = direction * speed;

        // 플레이어 컨트롤러 캐싱
        playerController = owner.GetComponent<PlayerController>();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        if (isHit || owner == null) return;

        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            isHit = true;
            owner.ResetGrapple();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHit) return;

        if (collision.CompareTag("BigEnemy"))
        {
            isHit = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

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
        else if (collision.CompareTag("BulletItem"))
        {
            BulletItem item = collision.GetComponent<BulletItem>();
            if (item != null)
            {
                PlayerRangedAttack attack = owner.GetComponent<PlayerRangedAttack>();
                item.StartCollection(attack);
            }
            owner.ResetGrapple();
            Destroy(gameObject);
        }
    }

    IEnumerator FlyToPoint(Vector2 targetPoint, Transform enemyTransform)
    {
        Rigidbody2D playerRb = owner.GetComponent<Rigidbody2D>();
        float originalGravity = playerRb.gravityScale;

        owner.isPullingEnemy = true;
        playerRb.gravityScale = 0f;

        Collider2D playerCol = owner.GetComponent<Collider2D>();
        Collider2D enemyCol = enemyTransform.GetComponent<Collider2D>();
        if (playerCol != null && enemyCol != null)
            Physics2D.IgnoreCollision(playerCol, enemyCol, true);

        float flySpeed = 30f;
        float arrivalDistance = 1.0f;

        while (enemyTransform != null && Vector2.Distance(owner.transform.position, targetPoint) > arrivalDistance)
        {
            Vector2 dir = (targetPoint - (Vector2)owner.transform.position).normalized;
            playerRb.linearVelocity = dir * flySpeed;
            transform.position = targetPoint;
            yield return null;
        }

        // [마무리 및 공격 예약 실행]
        playerRb.linearVelocity = Vector2.zero;
        playerRb.gravityScale = originalGravity;

        if (playerCol != null && enemyCol != null)
            Physics2D.IgnoreCollision(playerCol, enemyCol, false);

        owner.isPullingEnemy = false;

        // 도착 직후 예약된 공격이 있다면 실행
        if (playerController != null) playerController.ExecuteReservedAttack();

        owner.ResetGrapple();
        Destroy(gameObject);
    }

    IEnumerator PullEnemy(Transform enemyTransform)
    {
        Rigidbody2D enemyRb = enemyTransform.GetComponent<Rigidbody2D>();
        EnemyAllyAI enemyAI = enemyTransform.GetComponent<EnemyAllyAI>();

        Collider2D enemyCol = enemyTransform.GetComponent<Collider2D>();
        Collider2D playerCol = owner.GetComponent<Collider2D>();

        owner.isPullingEnemy = true;
        float fixedFacingDir = Mathf.Sign(owner.transform.localScale.x);
        float stopDistance = 1.5f;

        if (enemyAI != null) enemyAI.SetStun(true);

        if (enemyCol != null && playerCol != null)
            Physics2D.IgnoreCollision(playerCol, enemyCol, true);

        while (enemyTransform != null)
        {
            Vector2 targetPos = (Vector2)owner.transform.position + new Vector2(fixedFacingDir * stopDistance, 0);
            float distanceToTarget = Vector2.Distance(targetPos, enemyTransform.position);

            if (distanceToTarget < 0.2f) break;

            Vector2 pullDirection = (targetPos - (Vector2)enemyTransform.position).normalized;
            if (enemyRb != null) enemyRb.linearVelocity = pullDirection * pullSpeed;

            transform.position = enemyTransform.position;
            yield return null;
        }

        if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;

        if (enemyCol != null && playerCol != null)
            Physics2D.IgnoreCollision(playerCol, enemyCol, false);

        if (enemyAI != null) enemyAI.SetStun(false);

        owner.isPullingEnemy = false; // [중요] 예약 공격 실행 전에 false로 변경

        // 당겨온 직후 예약된 공격이 있다면 실행
        if (playerController != null) playerController.ExecuteReservedAttack();

        owner.ResetGrapple();
        Destroy(gameObject);
    }
}