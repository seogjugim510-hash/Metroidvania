using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 사용을 위해 필요

public class GameClearManager : MonoBehaviour
{
    public static GameClearManager Instance { get; private set; }

    [Header("UI 패널")]
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private float delayTime = 1.0f; // 지연 시간 설정

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameClearPanel != null)
            gameClearPanel.SetActive(false);
    }

    // 외부에서 호출하는 함수
    public void OnGameClear()
    {
        // 직접 UI를 켜는 대신 코루틴을 실행합니다.
        StartCoroutine(ShowClearPanelWithDelay());
    }

    // 실질적으로 1초 뒤에 UI를 띄우는 로직
    private IEnumerator ShowClearPanelWithDelay()
    {
        // 지정된 시간(1.0초)만큼 대기
        yield return new WaitForSeconds(delayTime);

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);

            // UI가 뜬 후에 게임을 멈춥니다.
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Mainmenu");
    }
}