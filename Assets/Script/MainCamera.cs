using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [Header("추적 설정")]
    [SerializeField] private Transform target;      // 따라갈 대상 (Player)
    [SerializeField] private float smoothSpeed = 0.125f; // 카메라 이동 부드러움 (낮을수록 부드러움)
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10); // 플레이어와의 거리 유지

    [Header("이동 제한 (선택 사항)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minPos; // 카메라가 갈 수 있는 최소 X, Y
    [SerializeField] private Vector2 maxPos; // 카메라가 갈 수 있는 최대 X, Y

    void LateUpdate() // 물리 이동 후에 카메라가 움직여야 떨림이 적습니다.
    {
        if (target == null) return;

        // 1. 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 2. 부드러운 이동 (Lerp 사용)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 3. 이동 범위 제한 (맵 밖으로 안 나가게 하고 싶을 때)
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minPos.x, maxPos.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minPos.y, maxPos.y);
        }

        // 4. 카메라 위치 적용
        transform.position = smoothedPosition;
    }
}