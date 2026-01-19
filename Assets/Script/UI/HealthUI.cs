using UnityEngine;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartContainer;
    [SerializeField] private int maxHearts = 3; // 최대 체력에 따른 하트 개수 (예: 30이면 3개)

    private List<GameObject> hearts = new List<GameObject>();

    void Awake()
    {
        // 처음에 최대 개수만큼 미리 생성해서 숨겨둠
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            heart.transform.localScale = Vector3.one;
            heart.SetActive(false);
            hearts.Add(heart);
        }
    }

    public void SetHealthDisplay(int currentHealth)
    {
        int targetHeartCount = currentHealth / 10;

        for (int i = 0; i < hearts.Count; i++)
        {
            // 목표 개수보다 작으면 켜고, 크면 끔
            // 리스트의 앞쪽(왼쪽)부터 켜지므로 자연스럽게 오른쪽부터 사라짐
            if (i < targetHeartCount)
            {
                hearts[i].SetActive(true);
            }
            else
            {
                hearts[i].SetActive(false);
            }
        }
    }
}