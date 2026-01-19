using System.Collections;
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
    private Coroutine lengthenCoroutine;

    [Header("거리 설정")]
    public float maxDistance = 15f; // [추가] 시각화 스크립트가 참조할 변수
    public float minDistance = 2f;

    [Header("스윙 안전 설정")]
    [SerializeField] private float safetyMargin = 2.5f; // 목표 공중 높이를 더 높게 잡습니다 (기존 1.5 -> 2.5 권장)
    [SerializeField] private float shortenOffset = 0.5f; // 계산된 길이에서 추가로 더 줄일 미터(m)
    [SerializeField] private LayerMask groundAndWallLayer; // 감지할 바닥 및 벽 레이어 마스크

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
        float verticalDist = Mathf.Abs(anchorPoint.y - transform.position.y);

        // 1. 목표 길이 계산 (삼각함수 방식 유지)
        float targetDistance = actualDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, safetyMargin + 1f, groundAndWallLayer);

        if (groundHit.collider != null && groundHit.distance < safetyMargin)
        {
            float heightToRaise = safetyMargin - groundHit.distance;
            float cosTheta = Mathf.Max(verticalDist / actualDistance, 0.1f);
            float targetVerticalDist = verticalDist - heightToRaise;

            targetDistance = Mathf.Clamp(targetVerticalDist / cosTheta, minDistance, actualDistance);
        }

        // 2. 물리 조인트 활성화 (현재 거리에서 시작)
        joint.distance = actualDistance;
        joint.enabled = true;
        isGrappling = true;

        // 3. 부드럽게 줄이기 시작 (코루틴)
        if (lengthenCoroutine != null) StopCoroutine(lengthenCoroutine);
        lengthenCoroutine = StartCoroutine(SmoothReel(targetDistance));

        // 4. 초기 가속도 (솟구침이 문제라면 이 힘을 낮추거나 제거하세요)
        rb.AddForce(Vector2.right * (transform.position.x < anchorPoint.x ? 3f : -3f), ForceMode2D.Impulse);
    }

    private IEnumerator SmoothReel(float targetLen)
    {
        float duration = 0.2f; // 0.2초 동안 부드럽게 줄어듦
        float elapsed = 0f;
        float startLen = joint.distance;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 선형 보간(Lerp)을 사용하여 길이를 서서히 변경
            joint.distance = Mathf.Lerp(startLen, targetLen, elapsed / duration);
            yield return null;
        }
        joint.distance = targetLen;
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