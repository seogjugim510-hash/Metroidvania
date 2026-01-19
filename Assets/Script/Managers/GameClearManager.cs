using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClearManager : MonoBehaviour
{
    [Header("UI 패널")]
    [SerializeField] private GameObject gameClearPanel; // 게임클리어 시 나타날 부모 오브젝트

    void Awake()
    {
        // 시작할 때는 클리어 창을 숨깁니다.
        if (gameClearPanel != null)
            gameClearPanel.SetActive(false);
    }

    // 빅에너미가 죽었을 때 호출될 함수
    public void OnGameClear()
    {
        gameClearPanel.SetActive(true); // 클리어 UI 출력
        Time.timeScale = 0f; // 게임 일시정지
    }

    // 다음 스테이지로 이동 (씬 빌드 순서상 다음 씬)
    public void ClickRestart()
    {
        Time.timeScale = 1f; // 일시정지 해제
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 메인 메뉴 버튼
    public void ClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Mainmenu");
    }
}