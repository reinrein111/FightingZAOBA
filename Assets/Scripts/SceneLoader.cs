using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        LoadingScreenManager.sceneToLoad = sceneName;
        SceneManager.LoadScene(SceneNames.LOADING);
    }

    public void LoadMainMenu() => LoadSceneByName(SceneNames.MAIN_MENU);
    public void LoadMap0() => LoadSceneWithLoadingScreen(SceneNames.MAP_0);
    public void LoadMap1() => LoadSceneWithLoadingScreen(SceneNames.MAP_1);
    public void LoadMap2() => LoadSceneWithLoadingScreen(SceneNames.MAP_2);
}
