using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon; // 이 아이템이 슬롯에 표시될 이미지
    public int attackPower; // 예시 스탯 (공격력 등)
}