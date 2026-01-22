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

    public bool isGrappling = false;
    private bool isLaunching = false;

    private Vector2 anchorPoint;

    [Header("거리 설정 (스케일 반영 전 기본값)")]
    public float maxDistance = 5f;
    public float minDistance = 2f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        line = GetComponent<LineRenderer>();
        joint.enabled = false;

        // 스케일이 5이므로 조인트의 감도를 위해 이 값을 조절할 수 있습니다.
        joint.enableCollision = true;
    }

    public bool isPullingEnemy = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isLaunching && !isGrappling)
        {
            LaunchProjectile();
        }

        if (Input.GetMouseButtonUp(1) && isGrappling && !isPullingEnemy)
        {
            ResetGrapple();
        }

        // [추가] 최소 거리 보정 로직
        if (isGrappling && joint.enabled)
        {
            HandleMinDistance();
        }

        if (isLaunching || isGrappling)
        {
            DrawLine();
        }
    }

    // 캐릭터 스케일에 따른 최소 거리 유지 로직
    void HandleMinDistance()
    {
        float currentDist = Vector2.Distance(transform.position, anchorPoint);

        // 현재 스케일에 맞춘 실제 최소 거리 (기본값 * 스케일)
        // 만약 인스펙터에서 이미 큰 값을 넣었다면 그대로 사용해도 됩니다.
        float actualMinDistance = minDistance;

        if (currentDist < actualMinDistance)
        {
            // 방법 1: 최소 거리에서 더 이상 줄어들지 않게 조인트 거리 고정
            joint.distance = actualMinDistance;

            // 방법 2: 너무 가까우면 자동으로 훅 해제 (원하시는 방식에 따라 선택)
            // if (currentDist < actualMinDistance * 0.8f) ResetGrapple();
        }
    }

    void LaunchProjectile()
    {
        isLaunching = true;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        currentProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        GrapplingProjectile projectileScript = currentProjectile.GetComponent<GrapplingProjectile>();
        projectileScript.maxDistance = this.maxDistance;
        projectileScript.Initialize(this, direction);

        line.positionCount = 2;
    }

    public void OnProjectileHit(Vector2 hitPoint)
    {
        isLaunching = false;

        if (!Input.GetMouseButton(1))
        {
            ResetGrapple();
            return;
        }

        isGrappling = true;
        anchorPoint = hitPoint;
        joint.connectedAnchor = anchorPoint;

        // [수정] 조인트 연결 시점의 거리가 최소 거리보다 작으면 최소 거리로 고정
        float hitDistance = Vector2.Distance(transform.position, anchorPoint);
        joint.distance = Mathf.Max(hitDistance, minDistance);

        joint.enabled = true;
    }

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
            // 스케일 5의 캐릭터를 움직이려면 더 큰 힘이 필요할 수 있습니다.
            rb.AddForce(new Vector2(x * swingForce, 0), ForceMode2D.Force);
        }
    }

    void DrawLine()
    {
        if (currentProjectile == null) return;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, currentProjectile.transform.position);
    }
}