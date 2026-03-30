/**
 * @file SpikeTrigger.cs
 * @brief 处理人物死亡逻辑，新增脚本
 *        死亡逻辑：用户触碰尖刺->用户消失->屏幕由红色变成透明，同时相机抖动->游戏重置\
 *        遗留问题：未作边界检测。新建了一个长条透明物体充当边界，与Spike共享同一代码
 * @author ZHY
 * @version 1.3
 * @time 26-3-30 0-0-03 ~~ 26-3-30 0-
 */
using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 用于 Image 和 CanvasGroup

public class SpikeTrigger : MonoBehaviour 
{
    [Header("References")]
    public Camera mainCamera;
    public CanvasGroup redOverlay; // 直接引用 CanvasGroup（需在 Unity 编辑器中拖拽赋值）

    [Header("Settings")]
    public float shakeDuration = 1f;
    public float shakeStrength = 0.4f;
    public float fadeDuration = 1f;
    public float resetDelay = 0.5f;
    private PlayerController player; 

    void Start() 
    {
        mainCamera = Camera.main;
        if (mainCamera == null) 
        {
            Debug.LogError("未找到 MainCamera！");
            return;
        }

        redOverlay = GameObject.FindWithTag("Overlay")?.GetComponent<CanvasGroup>();
        redOverlay.alpha = 0f; // 初始透明
        

        player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (player == null) 
        {
            Debug.LogError("未找到 Player 或 PlayerController 组件！");
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("Player")) 
        {
            Debug.Log("触发碰撞Trigger");
            StartCoroutine(TriggerSequence());
        }
    }

    IEnumerator TriggerSequence() 
    {
        Debug.Log("进入并行程序");
        player.gameObject.SetActive(false);

        // 启动相机抖动（并行执行）
        StartCoroutine(ShakeCamera());

        // 启动红色幕布淡入淡出，并等待完成
        yield return StartCoroutine(FadeRedOverlay());

        yield return new WaitForSeconds(resetDelay);
        ResetGame();
    }

    IEnumerator ShakeCamera() 
    {
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
        Debug.Log("屏幕抖动执行完毕！");
    }

    IEnumerator FadeRedOverlay() 
    {
        Debug.Log("开始执行红色幕布");
        if (redOverlay != null) 
        {
            redOverlay.alpha = 1f; // 瞬间变红

            float fadeOutElapsed = 0f;
            while (fadeOutElapsed < fadeDuration) 
            {
                redOverlay.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
                fadeOutElapsed += Time.deltaTime;
                yield return null;
            }
            redOverlay.alpha = 0f; // 确保最终透明
        }
    }

    void ResetGame() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

