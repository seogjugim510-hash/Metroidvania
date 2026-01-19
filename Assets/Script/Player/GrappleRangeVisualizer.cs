using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleRangeVisualizer : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GrapplingHook grapplingHook;
    [SerializeField] private int segments = 50;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color rangeColor = new Color(0, 1, 1, 0.4f);

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        // 중요: WorldSpace를 true로 하고 코드로 위치를 계산하는 것이 더 정확합니다.
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = rangeColor;
        lineRenderer.endColor = rangeColor;

        // 정렬 순서를 낮춰서 캐릭터 뒤에 그려지게 함
        lineRenderer.sortingOrder = -1;
    }

    void Update()
    {
        if (grapplingHook == null) return;

        // 훅이 발사된 상태(isGrappling)일 때 원을 숨기고 싶다면 아래 주석을 해제하세요.
        // if (grapplingHook.isGrappling) { lineRenderer.enabled = false; return; }
        // else { lineRenderer.enabled = true; }

        DrawRangeCircle(grapplingHook.maxDistance);
    }

    void DrawRangeCircle(float radius)
    {
        float angle = 0f;
        Vector3 center = transform.position; // 플레이어(부모)의 현재 위치

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            // 센터 값을 더해 플레이어를 따라다니게 함
            lineRenderer.SetPosition(i, new Vector3(center.x + x, center.y + y, 0));
            angle += (360f / segments);
        }
    }
}