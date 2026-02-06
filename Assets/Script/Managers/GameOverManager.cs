using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 사용을 위해 필수

public class GameOverManager : MonoBehaviour
{
    // --- 싱글톤 인스턴스 ---
    public static GameOverManager Instance { get; private set; }

    [Header("UI 패널")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float delayTime = 1.0f; // 게임오버 UI 지연 시간

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // 플레이어가 죽었을 때 외부(PlayerHealth 등)에서 호출할 함수
    public void OnPlayerDeath()
    {
        // 이미 게임오버가 진행 중이면 중복 실행 방지
        if (gameOverPanel.activeSelf) return;

        StartCoroutine(ShowGameOverPanelWithDelay());
    }

    private IEnumerator ShowGameOverPanelWithDelay()
    {
        // 1. 플레이어의 죽는 모습을 볼 수 있도록 잠시 대기
        yield return new WaitForSeconds(delayTime);

        // 2. UI 활성화
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // 3. 게임 정지 및 커서 활성화
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClickRestart()
    {
        Time.timeScale = 1f; // 반드시 1로 복구해야 씬이 흐릅니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Mainmenu");
    }
}