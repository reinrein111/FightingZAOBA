// /**
//  * @file SpikeTrigger.cs
//  * @brief 处理人物死亡逻辑，新增脚本
//  *        死亡逻辑：用户触碰尖刺->用户消失->屏幕由红色变成透明，同时相机抖动->游戏重置\
//  *        遗留问题：未作边界检测。新建了一个长条透明物体充当边界，与Spike共享同一代码
//  * @author ZHY
//  * @version 1.3
//  * @time 26-3-30 0-0-03 ~~ 26-3-30 0-
//  */
// using UnityEngine;
// using System.Collections;
// using UnityEngine.UI; // 用于 Image 和 CanvasGroup

// public class SpikeTrigger : MonoBehaviour 
// {
//     [Header("References")]
//     public Camera mainCamera;
//     public CanvasGroup redOverlay; // 直接引用 CanvasGroup（需在 Unity 编辑器中拖拽赋值）

//     [Header("Settings")]
//     public float shakeDuration = 1f;
//     public float shakeStrength = 0.4f;
//     public float fadeDuration = 1f;
//     public float resetDelay = 0.5f;
//     private PlayerController player; 

//     void Start() 
//     {
//         mainCamera = Camera.main;
//         if (mainCamera == null) 
//         {
//             Debug.LogError("未找到 MainCamera！");
//             return;
//         }

//         redOverlay = GameObject.FindWithTag("Overlay")?.GetComponent<CanvasGroup>();
//         redOverlay.alpha = 0f; // 初始透明
        

//         player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
//         if (player == null) 
//         {
//             Debug.LogError("未找到 Player 或 PlayerController 组件！");
//             return;
//         }
//     }

//     // void OnTriggerEnter2D(Collider2D collision) 
//     // {
//     //     if (collision.CompareTag("Player")) 
//     //     {
//     //         Debug.Log("触发碰撞Trigger");
//     //         StartCoroutine(TriggerSequence());
//     //     }
//     // }
//     private bool hasTriggered = false; // 加这行

//     void OnTriggerEnter2D(Collider2D collision) 
//     {
//         if (hasTriggered) return; // 加这行

//         if (collision.CompareTag("Player")) 
//         {
//             hasTriggered = true; // 加这行
//             Debug.Log("触发碰撞Trigger");
//             StartCoroutine(TriggerSequence());
//         }
//     }//chen yonhao修改for死亡触发bug

//     IEnumerator TriggerSequence() 
//     {
//         Debug.Log("进入并行程序");
//         player.gameObject.SetActive(false);

//         // 启动相机抖动（并行执行）
//         StartCoroutine(ShakeCamera());

//         // 启动红色幕布淡入淡出，并等待完成
//         yield return StartCoroutine(FadeRedOverlay());

//         yield return new WaitForSeconds(resetDelay);
//         ResetGame();
//     }

//     IEnumerator ShakeCamera() 
//     {
//         Vector3 originalPos = mainCamera.transform.position;
//         float elapsed = 0f;

//         while (elapsed < shakeDuration) 
//         {
//             float x = Random.Range(-1f, 1f) * shakeStrength;
//             float y = Random.Range(-1f, 1f) * shakeStrength;
//             mainCamera.transform.position = originalPos + new Vector3(x, y, 0);
//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         mainCamera.transform.position = originalPos;
//         Debug.Log("屏幕抖动执行完毕！");
//     }

//     IEnumerator FadeRedOverlay() 
//     {
//         Debug.Log("开始执行红色幕布");
//         if (redOverlay != null) 
//         {
//             redOverlay.alpha = 1f; // 瞬间变红

//             float fadeOutElapsed = 0f;
//             while (fadeOutElapsed < fadeDuration) 
//             {
//                 redOverlay.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
//                 fadeOutElapsed += Time.deltaTime;
//                 yield return null;
//             }
//             redOverlay.alpha = 0f; // 确保最终透明
//         }
//     }

//     void ResetGame() 
//     {
//         UnityEngine.SceneManagement.SceneManager.LoadScene(
//             UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
//         );
//     }
// }


// // 提供给子弹或其他脚本手动调用的接口
// public void ExecuteDeath()
// {
//     if (!hasTriggered)
//     {
//         hasTriggered = true;
//         Debug.Log("通过代码触发死亡逻辑");
//         StartCoroutine(TriggerSequence());
//     }
// }//chen yonghao修改for外部调用死亡逻辑接口



using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpikeTrigger : MonoBehaviour 
{
    [Header("References")]
    public Camera mainCamera;
    public CanvasGroup redOverlay; 

    [Header("Settings")]
    public float shakeDuration = 1f;
    public float shakeStrength = 0.4f;
    public float fadeDuration = 1f;
    public float resetDelay = 0.5f;

    private PlayerController player; 
    private bool hasTriggered = false;

    void Start() 
    {
        // 自动获取相机
        if (mainCamera == null) mainCamera = Camera.main;

        // 自动获取红色遮罩（需确保 UI 图片有 Overlay 标签）
        if (redOverlay == null)
        {
            GameObject overlayObj = GameObject.FindWithTag("Overlay");
            if (overlayObj != null) redOverlay = overlayObj.GetComponent<CanvasGroup>();
        }
        
        if (redOverlay != null) redOverlay.alpha = 0f;

        // 自动获取玩家
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
    }

    // --- 新增：供子弹调用的公开接口 ---
    public void ExecuteDeath()
    {
        if (hasTriggered) return;

        // 【预留点】：以后加包子护盾在这里 return
        // if (player.hasBaozi) { player.ConsumeBaozi(); return; }

        hasTriggered = true;
        StartCoroutine(TriggerSequence());
    }

    // 保留：地刺直接碰撞触发逻辑
    void OnTriggerEnter2D(Collider2D collision) 
    {
        if (hasTriggered) return;

        if (collision.CompareTag("Player")) 
        {
            ExecuteDeath();
        }
    }

    IEnumerator TriggerSequence() 
    {
        if (player != null) player.gameObject.SetActive(false);

        // 启动效果
        StartCoroutine(ShakeCamera());
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