/**
 * @file SpikeTrigger.cs
 * @brief 双人游戏尖刺触发器 - 任一玩家触碰尖刺即失败
 * @author ZHY
 * @version 3.0
 * @time 26-4-1
 */
using UnityEngine;
using System.Collections;

public class SpikeTrigger : MonoBehaviour 
{
    [Header("UI引用")]
    public CanvasGroup redOverlay;
    
    [Header("设置")]
    public float shakeDuration = 0.5f;
    public float shakeStrength = 0.1f;
    public float fadeDuration = 0.3f;
    public float resetDelay = 0.5f;
    
    private Camera mainCamera;
    private bool isTriggered = false;

    void Start() 
    {
        mainCamera = Camera.main;
        
        if (redOverlay == null)
        {
            redOverlay = GameObject.FindWithTag("Overlay")?.GetComponent<CanvasGroup>();
        }
        
        if (redOverlay != null)
        {
            redOverlay.alpha = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    {
        if (isTriggered) return;
        
        // 检查是否是玩家
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null) 
        {
            Debug.Log($"玩家{player.playerId}触碰尖刺！");
            isTriggered = true;
            StartCoroutine(TriggerSequence(player));
        }
    }

    IEnumerator TriggerSequence(PlayerController player) 
    {
        // 禁用触发玩家
        player.gameObject.SetActive(false);
        
        // 启动相机抖动（并行执行）
        StartCoroutine(ShakeCamera());
        
        // 启动红色幕布淡入淡出
        yield return StartCoroutine(FadeRedOverlay());
        
        yield return new WaitForSeconds(resetDelay);
        ResetGame();
    }

    IEnumerator ShakeCamera() 
    {
        if (mainCamera == null) yield break;
        
        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration) 
        {
            float x = Random.Range(-1f, 1f) * shakeStrength;
            float y = Random.Range(-1f, 1f) * shakeStrength;
            mainCamera.transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPos;
    }

    IEnumerator FadeRedOverlay() 
    {
        if (redOverlay != null) 
        {
            redOverlay.alpha = 1f;

            float fadeOutElapsed = 0f;
            while (fadeOutElapsed < fadeDuration) 
            {
                redOverlay.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
                fadeOutElapsed += Time.deltaTime;
                yield return null;
            }
            redOverlay.alpha = 0f;
        }
    }

    void ResetGame() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
