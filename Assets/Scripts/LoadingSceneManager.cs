using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static string sceneToLoad = SceneNames.MAP_0;
    
    [Header("UI引用")]
    public TextMeshProUGUI storyText;
    
    [Header("打字机设置")]
    [Tooltip("每个字符的显示间隔（秒）")]
    public float typewriterSpeed = 0.05f;
    
    [Tooltip("完整故事文本")]
    [TextArea(10, 20)]
    public string fullStory;
    
    private Coroutine typewriterCoroutine;
    private bool isTyping;
    private int totalCharacters;
    
    private void Start()
    {
        if (storyText == null)
        {
            Debug.LogError("LoadingScreenManager: storyText 未赋值！");
            return;
        }
        
        storyText.text = fullStory;
        storyText.ForceMeshUpdate(true, true);
        totalCharacters = storyText.textInfo.characterCount;
        storyText.maxVisibleCharacters = 0;
        
        typewriterCoroutine = StartCoroutine(TypewriterEffect());
    }
    
    private IEnumerator TypewriterEffect()
    {
        isTyping = true;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            storyText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    public void SkipStory()
    {
        if (isTyping)
        {
            StopCoroutine(typewriterCoroutine);
            storyText.maxVisibleCharacters = totalCharacters;
            isTyping = false;
        }
        else
        {
            string targetScene = string.IsNullOrEmpty(sceneToLoad) ? SceneNames.MAP_0 : sceneToLoad;
            SceneManager.LoadScene(targetScene);
        }
    }
}
