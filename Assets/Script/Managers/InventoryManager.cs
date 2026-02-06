using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("탐색 설정")]
    [SerializeField] private int stepSize = 50; // 인접 슬롯 간 거리
    private Vector2Int corePos;
    private Dictionary<Vector2Int, InventorySlot> allSlots = new Dictionary<Vector2Int, InventorySlot>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void RegisterSlot(Vector2Int pos, InventorySlot slot)
    {
        if (!allSlots.ContainsKey(pos))
            allSlots.Add(pos, slot);
    }

    public void SetCorePosition(Vector2Int pos) => corePos = pos;

    public void RefreshConnections()
    {
        // 1. 초기화
        foreach (var slot in allSlots.Values) slot.UpdateConnection(false);

        if (!allSlots.ContainsKey(corePos)) return;

        // 2. BFS 탐색
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(corePos);
        visited.Add(corePos);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            InventorySlot slot = allSlots[current];

            slot.UpdateConnection(true);

            // 잠기지 않은 슬롯(통로)인 경우에만 인접 탐색
            if (slot.currentState != SlotState.Locked)
            {
                Vector2Int[] neighbors = {
                    current + new Vector2Int(0, stepSize),
                    current + new Vector2Int(0, -stepSize),
                    current + new Vector2Int(-stepSize, 0),
                    current + new Vector2Int(stepSize, 0)
                };

                foreach (var next in neighbors)
                {
                    if (allSlots.ContainsKey(next) && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
        }
    }
}