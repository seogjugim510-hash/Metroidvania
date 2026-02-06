using UnityEngine;
using UnityEditor;

public class InventoryGridGenerator : EditorWindow
{
    private GameObject slotPrefab;
    private Vector2Int gridCount = new Vector2Int(5, 5);
    private float stepSize = 50f;
    private Transform parent;

    [MenuItem("Tools/Inventory Grid Generator")]
    public static void ShowWindow() => GetWindow<InventoryGridGenerator>("Grid Gen");

    private void OnGUI()
    {
        slotPrefab = (GameObject)EditorGUILayout.ObjectField("Slot Prefab", slotPrefab, typeof(GameObject), false);
        gridCount = EditorGUILayout.Vector2IntField("Grid Count (X, Y)", gridCount);
        stepSize = EditorGUILayout.FloatField("Step Size", stepSize);
        parent = (Transform)EditorGUILayout.ObjectField("Parent Panel", parent, typeof(Transform), true);

        if (GUILayout.Button("Generate Grid"))
        {
            if (slotPrefab == null || parent == null) return;

            // 기존 자식들 삭제 (선택 사항)
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroyImmediate(parent.GetChild(i).gameObject);

            for (int y = 0; y < gridCount.y; y++)
            {
                for (int x = 0; x < gridCount.x; x++)
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, parent);
                    RectTransform rt = obj.GetComponent<RectTransform>();

                    // 중앙 정렬을 위해 (0,0)을 기준으로 배치
                    float posX = (x - (gridCount.x / 2f) + 0.5f) * stepSize;
                    float posY = (y - (gridCount.y / 2f) + 0.5f) * stepSize;

                    rt.anchoredPosition = new Vector2(posX, posY);
                    obj.name = $"Slot_{x}_{y}";
                }
            }
        }
    }
}