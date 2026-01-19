using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수

public class MainMenu : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string inGameSceneName = "Ingame"; // 이동할 인게임 씬 이름

    // Start 버튼에 연결할 함수
    public void ClickStart()
    {
        // 지정된 이름의 씬을 로드합니다.
        SceneManager.LoadScene(inGameSceneName);
    }

    // Exit 버튼에 연결할 함수
    public void ClickExit()
    {
#if UNITY_EDITOR
        // 에디터에서 테스트할 때 종료 확인용
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 게임 클라이언트 종료
        Application.Quit();
#endif
        Debug.Log("게임이 종료되었습니다.");
    }
}