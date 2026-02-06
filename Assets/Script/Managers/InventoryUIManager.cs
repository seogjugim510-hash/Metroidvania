using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("인벤토리 캔버스들")]
    [SerializeField] private GameObject inventoryCanvas1;
    [SerializeField] private GameObject inventoryCanvas2;

    private bool isInventoryOpen = false;
    public bool IsInventoryOpen => isInventoryOpen;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        CloseInventory();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInventoryOpen) CloseInventory();
            else OpenInventory();
        }
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
        inventoryCanvas1.SetActive(true);
        inventoryCanvas2.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        inventoryCanvas1.SetActive(false);
        inventoryCanvas2.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
    }
}