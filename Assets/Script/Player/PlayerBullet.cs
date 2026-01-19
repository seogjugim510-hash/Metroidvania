using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 2f; // 일정 시간 뒤 자동 소멸

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 총알이 생성될 때 캐릭터가 바라보는 방향으로 속도 설정
        rb.linearVelocity = transform.right * speed;

        // 2초 뒤에 총알 삭제
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 적에게 맞았을 때 (EnemyAllyAI 스크립트 기준)
        if (collision.CompareTag("Enemy") || collision.CompareTag("BigEnemy"))
        {
            EnemyAllyAI enemy = collision.GetComponent<EnemyAllyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // 적중 시 총알 삭제
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject); // 벽에 닿아도 삭제
        }
    }
}