using UnityEngine;
using UnityEngine.SceneManagement;

public class Skip : MonoBehaviour
{
    public void SkipToNextScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SkipToNextScene: sceneName 不能为空");
            return;
        }

        int buildIndex = SceneManager.GetSceneByName(sceneName).buildIndex;
        if (buildIndex == -1)
        {
            Debug.LogError($"SkipToNextScene: 场景 '{sceneName}' 未在 Build Settings 中启用");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
