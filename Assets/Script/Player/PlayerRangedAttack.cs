using UnityEngine;

public class PlayerRangedAttack : MonoBehaviour
{
    [Header("탄환 설정")]
    [SerializeField] private int currentAmmo = 0;
    [SerializeField] private int maxAmmo = 10;

    [Header("공격 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.4f;
    private float nextFireTime;
    private PlayerAnimation playerAnim;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;

    void Awake()
    {
        playerAnim = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        // 탄환이 있고, F키를 눌렀으며, 쿨타임이 지났을 때 발사
        if (Input.GetKeyDown(KeyCode.F) && currentAmmo > 0 && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        currentAmmo--; // 탄환 소모

        // 캐릭터 방향에 따라 총알 생성
        Quaternion rotation = transform.localScale.x > 0 ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
        Instantiate(bulletPrefab, firePoint.position, rotation);

        Debug.Log($"발사! 남은 탄환: {currentAmmo}/{maxAmmo}");
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo); // 최대치 고정
        Debug.Log($"탄환 획득! 현재 탄환: {currentAmmo}");
    }
}