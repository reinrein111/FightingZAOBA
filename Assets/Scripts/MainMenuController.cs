using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("默认加载的游戏场景")]
    public string defaultGameScene = SceneNames.MAP_0;

    public void StartGameWithLoadingScreen()
    {
        StartGameWithLoadingScreen(defaultGameScene);
    }

    public void StartGameWithLoadingScreen(string sceneName)
    {
        LoadingScreenManager.sceneToLoad = sceneName;
        SceneManager.LoadScene(SceneNames.LOADING);
    }

    public void LoadMap0() => StartGameWithLoadingScreen(SceneNames.MAP_0);
    public void LoadMap1() => StartGameWithLoadingScreen(SceneNames.MAP_1);
    public void LoadMap2() => StartGameWithLoadingScreen(SceneNames.MAP_2);

}