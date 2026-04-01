/**
 * @file ExitTrigger.cs
 * @brief 双人游戏出口触发器 - 任一玩家到达出口即胜利
 * @author ZHY
 * @version 3.0
 * @time 26-4-1
 */
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExitTrigger : MonoBehaviour 
{
    [Header("UI引用")]
    public CanvasGroup blackOverlay;
    
    [Header("设置")]
    public float fadeDuration = 1f;
    public float resetDelay = 0.5f;
    
    private bool isTriggered = false;

    void Start() 
    {
        if (blackOverlay == null)
        {
            blackOverlay = GameObject.FindWithTag("BlackOverlay")?.GetComponent<CanvasGroup>();
        }
        
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    {
        if (isTriggered) return;
        
        // 检查是否是玩家
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && player.isPlayerGrounded()) 
        {
            Debug.Log($"玩家{player.playerId}到达出口！");
            isTriggered = true;
            StartCoroutine(TriggerSequenceCoroutine(player));
        }
    }

    private IEnumerator TriggerSequenceCoroutine(PlayerController player) 
    {
        // 禁用触发玩家的控制
        player.gameObject.SetActive(false);
        
        yield return StartCoroutine(FadeBlackOverlayCoroutine());
        
        ResetGame();
    }

    private IEnumerator FadeBlackOverlayCoroutine() 
    {
        Debug.Log("开始执行黑色幕布淡出");
        if (blackOverlay != null) 
        {
            blackOverlay.alpha = 1f;
            
            float fadeOutElapsed = 0f;
            while (fadeOutElapsed < fadeDuration) 
            {
                blackOverlay.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
                fadeOutElapsed += Time.deltaTime;
                yield return null;
            }
            blackOverlay.alpha = 0f;
        }
    }

    void ResetGame() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
