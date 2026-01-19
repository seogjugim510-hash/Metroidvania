using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour
{
    public GameObject projectilePrefab;
    public LayerMask grappleLayer;
    public float swingForce = 20f;

    private DistanceJoint2D joint;
    private LineRenderer line;
    private Rigidbody2D rb;
    private GameObject currentProjectile;

    public bool isGrappling = false; // 훅이 어딘가에 걸려 있는 상태
    private bool isLaunching = false;  // 훅이 날아가고 있는 상태

    private Vector2 anchorPoint;

    [Header("거리 설정")]
    public float maxDistance = 15f;
    public float minDistance = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        line = GetComponent<LineRenderer>();
        joint.enabled = false;
    }

    public bool isPullingEnemy = false;

    void Update()
    {
        // 1. 발사 시도 (날아가고 있지 않을 때만 가능)
        if (Input.GetMouseButtonDown(1) && !isLaunching && !isGrappling)
        {
            LaunchProjectile();
        }

        // 2. 훅 유지 및 해제 로직
        // 발사가 끝난(isGrappling) 상태에서 마우스 우클릭을 떼면 즉시 해제
        // 단, 에너미를 당기는 중(isPullingEnemy)에는 예외 처리
        if (Input.GetMouseButtonUp(1) && isGrappling && !isPullingEnemy)
        {
            ResetGrapple();
        }

        // 시각화 업데이트
        if (isLaunching || isGrappling)
        {
            DrawLine();
        }
    }

    void LaunchProjectile()
    {
        isLaunching = true; // 날아가기 시작함 (클릭을 떼도 이 값은 유지됨)

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        currentProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        GrapplingProjectile projectileScript = currentProjectile.GetComponent<GrapplingProjectile>();
        projectileScript.maxDistance = this.maxDistance;
        projectileScript.Initialize(this, direction);

        line.positionCount = 2;
    }

    // 투사체가 무언가(벽/적/아이템)에 맞았을 때 투사체 스크립트에서 호출됨
    public void OnProjectileHit(Vector2 hitPoint)
    {
        isLaunching = false; // 날아가는 단계 종료

        // [핵심] 벽에 맞은 순간 마우스를 이미 떼고 있다면 즉시 취소
        if (!Input.GetMouseButton(1))
        {
            ResetGrapple();
            return;
        }

        // 마우스를 누르고 있다면 스윙 상태(isGrappling) 진입
        isGrappling = true;
        anchorPoint = hitPoint;
        joint.connectedAnchor = anchorPoint;
        joint.distance = Vector2.Distance(transform.position, anchorPoint);
        joint.enabled = true;

        // 초기 가속도 추가
        //if (rb.linearVelocity.magnitude < 2f)
        //{
        //    Vector2 pushDir = (transform.position.x < anchorPoint.x) ? Vector2.right : Vector2.left;
        //    rb.AddForce((Vector2.up + pushDir).normalized * 5f, ForceMode2D.Impulse);
        //}
    }

    // 투사체가 최대 사거리에 도달하여 빗나갔을 때 호출됨
    public void ResetGrapple()
    {
        isLaunching = false;
        isGrappling = false;
        isPullingEnemy = false;

        if (joint != null) joint.enabled = false;
        line.positionCount = 0;

        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
            currentProjectile = null;
        }
    }

    public bool IsHookActive() => currentProjectile != null;

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
        if (currentProjectile == null) return;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, currentProjectile.transform.position);
    }
}