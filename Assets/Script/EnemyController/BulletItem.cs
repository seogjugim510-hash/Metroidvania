using UnityEngine;

public class BulletItem : MonoBehaviour
{
    [Header("움직임 설정")]
    [SerializeField] private float moveDistance = 1.5f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("수집/전투 설정")]
    [SerializeField] private float flySpeed = 15f;
    [SerializeField] private int health = 10; // 블릿 아이템의 체력
    [SerializeField] private int collisionDamage = 10; // 플레이어와 충돌 시 입히는 데미지

    private Vector2 startPos;
    private bool isBeingCollected = false;
    private Transform targetPlayer;
    private PlayerRangedAttack playerScript;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (isBeingCollected)
        {
            FlyTowardsPlayer();
        }
        else
        {
            float newX = startPos.x + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
            transform.position = new Vector2(newX, transform.position.y);
        }
    }

    // [추가] 일반 공격 등으로 데미지를 입을 때 호출
    public void TakeDamage(int damage)
    {
        if (isBeingCollected) return; // 이미 수집 중이면 무적

        health -= damage;
        if (health <= 0)
        {
            // 체력이 다하면 총알 보급 없이 파괴
            Debug.Log("아이템이 파괴되었습니다. (보급 실패)");
            Destroy(gameObject);
        }
    }

    // 훅에 맞았을 때 호출 (기존과 동일)
    public void StartCollection(PlayerRangedAttack player)
    {
        if (isBeingCollected) return;

        playerScript = player;
        targetPlayer = player.transform;
        isBeingCollected = true;

        GetComponent<Collider2D>().enabled = false;
    }

    void FlyTowardsPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPlayer.position) < 0.2f)
        {
            playerScript.AddAmmo(5);
            Destroy(gameObject);
        }
    }

    // [추가] 플레이어와 직접 충돌했을 때의 로직
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 수집 중이 아닐 때 플레이어와 부딪히면
        if (!isBeingCollected && collision.CompareTag("Player"))
        {
            // 플레이어의 체력 스크립트를 찾아 데미지를 줌
            // (PlayerHealth 스크립트가 있다고 가정하거나 PlayerController의 데미지 함수 호출)
            var playerHealth = collision.GetComponent<PlayerHealth>(); // 혹은 본인의 체력 스크립트 명
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(collisionDamage);
            }

            Debug.Log("아이템과 충돌하여 데미지를 입었습니다!");
            Destroy(gameObject); // 아이템 소멸
        }
    }
}