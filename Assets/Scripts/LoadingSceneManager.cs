using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static string sceneToLoad = SceneNames.MAP_0;
    
    [Header("UI引用")]
    public Slider progressBar;
    public TextMeshProUGUI percentageText;
    
    [Header("设置")]
    public bool requireKeyPress = true;
    
    private void Start()
    {
        if (progressBar == null)
        {
            Debug.LogError("LoadingScreenManager: progressBar 未赋值！");
        }
        if (percentageText == null)
        {
            Debug.LogError("LoadingScreenManager: percentageText 未赋值！");
        }
        
        StartCoroutine(LoadSceneAsyncProcess(sceneToLoad));
    }
    
    private IEnumerator LoadSceneAsyncProcess(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadingScreenManager: sceneToLoad 为空，使用默认场景 MAP_0");
            sceneName = SceneNames.MAP_0;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        if (asyncLoad == null)
        {
            Debug.LogError($"LoadingScreenManager: 无法加载场景 '{sceneName}'，请检查场景是否已添加到 Build Settings！");
            yield break;
        }
        
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            if (percentageText != null)
            {
                percentageText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }
            
            if (asyncLoad.progress >= 0.9f)
            {
                if (requireKeyPress)
                {
                    if (percentageText != null)
                    {
                        percentageText.text = "Press Space to continue...";
                    }
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                else
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }
}