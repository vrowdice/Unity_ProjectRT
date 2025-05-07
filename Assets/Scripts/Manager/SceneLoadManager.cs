using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoadManager : MonoBehaviour
{
    //버튼별로 할당된 String에 해당하는 씬 불러오기
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

    // 씬 참조 오류 확인
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
