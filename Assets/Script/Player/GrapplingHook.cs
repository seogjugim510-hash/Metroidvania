using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public GameObject projectilePrefab; // 훅 프리팹
    public LayerMask grappleLayer;
    public float swingForce = 20f;

    private DistanceJoint2D joint;
    private LineRenderer line;
    private Rigidbody2D rb;
    private GameObject currentProjectile;
    public bool isGrappling = false;
    private Vector2 anchorPoint;

    [Header("거리 설정")]
    public float maxDistance = 15f; // [추가] 시각화 스크립트가 참조할 변수
    public float minDistance = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        line = GetComponent<LineRenderer>();
        joint.enabled = false;
    }
    public bool isPullingEnemy = false; // 에너미를 당기는 중인지 체크
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGrappling)
        {
            LaunchProjectile();
        }

        // [수정] 에너미를 당기는 중이 아닐 때만 마우스 떼기 입력 허용
        if (Input.GetMouseButtonUp(1) && !isPullingEnemy)
        {
            ResetGrapple();
        }

        if (isGrappling)
        {
            DrawLine();
        }
    }

    void LaunchProjectile()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        currentProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // [수정] 투사체에 GrapplingHook의 maxDistance를 전달합니다.
        GrapplingProjectile projectileScript = currentProjectile.GetComponent<GrapplingProjectile>();
        projectileScript.maxDistance = this.maxDistance;
        projectileScript.Initialize(this, direction);

        line.positionCount = 2;
        isGrappling = true;
    }

    public void OnProjectileHit(Vector2 hitPoint)
    {
        anchorPoint = hitPoint;
        joint.connectedAnchor = anchorPoint;

        float actualDistance = Vector2.Distance(transform.position, anchorPoint);
        joint.distance = Mathf.Max(actualDistance, minDistance);
        joint.enabled = true;

        // [추가] 진자 운동을 유도하기 위한 초기 충격
        // 현재 속도가 너무 느리다면 진행 방향이나 아래쪽으로 힘을 살짝 줍니다.
        if (rb.linearVelocity.magnitude < 2f)
        {
            // 플레이어가 앵커보다 아래에 있다면 옆으로, 수평에 가깝다면 아래로
            Vector2 pushDir = (transform.position.x < anchorPoint.x) ? Vector2.left : Vector2.right;
            rb.AddForce((Vector2.down + pushDir).normalized * 3f, ForceMode2D.Impulse);
        }
    }

    public bool IsHookActive()
    {
        // 투사체가 존재하면 true 반환
        return currentProjectile != null;
    }

    public void ResetGrapple()
    {
        isGrappling = false;
        isPullingEnemy = false;

        if (joint != null) joint.enabled = false;
        line.positionCount = 0;

        // 투사체 제거
        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
            currentProjectile = null;
        }
    }

    void FixedUpdate()
    {
        if (isGrappling && joint.enabled)
        {
            float x = Input.GetAxisRaw("Horizontal");
            rb.AddForce(new Vector2(x * swingForce, 0));
        }
    }

    void DrawLine()
    {
        line.SetPosition(0, transform.position);
        if (currentProjectile != null)
        {
            // 줄의 시각적 끝점은 항상 투사체 위치로 고정
            line.SetPosition(1, currentProjectile.transform.position);
        }
    }
}