using UnityEngine;
using UnityEngine.UI;

public enum SlotState { Locked, Unlocked, Equipped }

public class InventorySlot : MonoBehaviour
{
    [Header("설정")]
    public bool isStartingCore = false;
    public SlotState currentState = SlotState.Locked;

    [Header("아이템 데이터")]
    public ItemData currentItem; // 현재 이 슬롯에 들어있는 아이템 정보

    [Header("UI 연결")]
    [SerializeField] private Image slotImage;     // 슬롯 배경
    [SerializeField] private Image iconImage;     // 아이템 아이콘 이미지 (중요: Image 컴포넌트)
    [SerializeField] private GameObject itemIconObj; // 아이템 아이콘의 부모 오브젝트

    [Header("상태별 색상")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color equippedColor = Color.cyan;
    [SerializeField] private Color disconnectedColor =  new Color(0.3f, 0.3f, 0.3f, 1f);

    [HideInInspector] public Vector2Int gridPos;
    [HideInInspector] public bool isConnectedToCore = false;

    private void Awake()
    {
        RectTransform rt = GetComponent<RectTransform>();
        int x = Mathf.RoundToInt(rt.anchoredPosition.x / 50f) * 50;
        int y = Mathf.RoundToInt(rt.anchoredPosition.y / 50f) * 50;
        gridPos = new Vector2Int(x, y);
        rt.anchoredPosition = new Vector2(x, y);
    }

    private void Start()
    {
        InventoryManager.Instance.RegisterSlot(gridPos, this);

        if (isStartingCore)
        {
            InventoryManager.Instance.SetCorePosition(gridPos);

            // 코어는 강제로 Unlocked 상태로 고정 (아이템 장착 불가)
            currentState = SlotState.Unlocked;

            // [중요] 코어에 버튼이 있다면 비활성화, 없다면 에러 방지
            Button btn = GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }

        UpdateVisual();
    }

    public void OnSlotClick()
    {
        // 코어는 클릭해도 반응하지 않도록 한 번 더 방어
        if (isStartingCore) return;

        if (!InventoryUIManager.Instance.IsInventoryOpen) return;

        switch (currentState)
        {
            case SlotState.Locked: TryUnlock(); break;
            case SlotState.Unlocked: TryEquip(); break;
            case SlotState.Equipped: TryUnequip(); break;
        }
    }

    private void TryUnlock() => SetState(SlotState.Unlocked);

    // 테스트용: 인스펙터에 등록된 currentItem을 장착합니다.
    private void TryEquip()
    {
        if (currentItem != null)
        {
            iconImage.sprite = currentItem.itemIcon; // 아이콘 교체
            SetState(SlotState.Equipped);
        }
        else
        {
            Debug.LogWarning("장착할 아이템 데이터가 없습니다!");
        }
    }

    private void TryUnequip() => SetState(SlotState.Unlocked);

    public void SetState(SlotState newState)
    {
        currentState = newState;
        InventoryManager.Instance.RefreshConnections();
    }

    public void UpdateConnection(bool connected)
    {
        isConnectedToCore = connected;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Color baseColor = currentState switch
        {
            SlotState.Locked => lockedColor,
            SlotState.Unlocked => unlockedColor,
            SlotState.Equipped => equippedColor,
            _ => Color.white
        };

        if (currentState != SlotState.Locked && !isConnectedToCore)
            slotImage.color = baseColor * disconnectedColor;
        else
            slotImage.color = baseColor;

        // 장착 중이고 아이템 데이터가 있을 때만 아이콘 표시
        itemIconObj.SetActive(currentState == SlotState.Equipped && currentItem != null);
    }
}