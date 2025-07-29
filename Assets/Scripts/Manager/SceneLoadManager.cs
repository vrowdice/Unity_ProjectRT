using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// 씬 로딩을 관리하는 매니저 클래스
/// 씬 전환 및 빌드 설정 검증을 담당
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
    /// <summary>
    /// 버튼에서 할당된 String에 해당하는 씬을 로드합니다
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadScene(string sceneName)
    {
        if (IsSceneInBuildSettings(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"씬 '{sceneName}' 이(가) Build Settings에 없습니다.");
        }
    }

    /// <summary>
    /// 씬이 빌드 설정에 있는지 확인
    /// </summary>
    /// <param name="sceneName">확인할 씬 이름</param>
    /// <returns>빌드 설정에 있으면 true, 없으면 false</returns>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }
}
