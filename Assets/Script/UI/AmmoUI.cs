using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 필요

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private PlayerRangedAttack playerAttack;

    void Update()
    {
        if (playerAttack != null && ammoText != null)
        {
            // "Ammo: 5 / 10" 형식으로 표시
            ammoText.text = $"Ammo: {playerAttack.GetCurrentAmmo()} / {playerAttack.GetMaxAmmo()}";
        }
    }
}