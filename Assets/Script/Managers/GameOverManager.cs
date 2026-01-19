using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환용
using UnityEngine.UI; // UI 제어용 (필요 시)

public class GameOverManager : MonoBehaviour
{
    [Header("UI 패널")]
    [SerializeField] private GameObject gameOverPanel; // 게임오버 시 나타날 부모 오브젝트

    void Awake()
    {
        // 시작할 때는 게임오버 창을 숨깁니다.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // 플레이어가 죽었을 때 호출될 함수
    public void OnPlayerDeath()
    {
        gameOverPanel.SetActive(true); // UI 출력
        Time.timeScale = 0f; // 게임 일시정지 (선택 사항)
    }

    // 다시 시작 버튼 (현재 씬 재로드)
    public void ClickRestart()
    {
        Time.timeScale = 1f; // 일시정지 해제
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 메인 메뉴 버튼
    public void ClickMainMenu()
    {
        Time.timeScale = 1f; // 일시정지 해제
        SceneManager.LoadScene("Mainmenu"); // 메인메뉴 씬 이름으로 수정하세요
    }
}