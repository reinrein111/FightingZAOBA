// /**
//  * @file ExitTrigger.cs
//  * @brief 增添主角胜利逻辑
//  *        胜利逻辑：碰撞出口并且角色处于着陆状态，黑屏闪烁后重启游戏
//  * @author ZHY
//  * @version 1.3(DJH修改后，ZHY再作修改版本)
//  * @time 26-3-30 0-0-03 ~~ 26-3-30 0-
//  */
// using UnityEngine;
// using System.Collections;
// using UnityEngine.UI;

// public class ExitTrigger : MonoBehaviour 
// {
//     [Header("References")]
//     public Camera mainCamera;
//     public CanvasGroup blackOverlay;

//     [Header("Settings")]
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

//         blackOverlay = GameObject.FindWithTag("BlackOverlay")?.GetComponent<CanvasGroup>();
//         if (blackOverlay != null)
//         {
//             blackOverlay.alpha = 0f; // 初始透明
//         }
//         else
//         {
//             Debug.LogError("未找到 BlackOverlay 或 CanvasGroup 组件！");
//         }

//         player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
//         if (player == null) 
//         {
//             Debug.LogError("未找到 Player 或 PlayerController 组件！");
//             return;
//         }
//     }

//     void OnTriggerEnter2D(Collider2D collision) 
//     {
//         if (collision.CompareTag("Player") && player.isPlayerGrounded()) 
//         {
//             Debug.Log("触发碰撞Trigger");
//             StartCoroutine(TriggerSequenceCoroutine());
//         }
//     }

//     private IEnumerator TriggerSequenceCoroutine() 
//     {
//         player.gameObject.SetActive(false);
//         yield return StartCoroutine(FadeBlackOverlayCoroutine());
//         ResetGame();
//     }

//     private IEnumerator FadeBlackOverlayCoroutine() 
//     {
//         Debug.Log("开始执行黑色幕布淡出");
//         if (blackOverlay != null) 
//         {
//             // 先瞬间变黑
//             blackOverlay.alpha = 1f;
            
//             // 然后淡出
//             float fadeOutElapsed = 0f;
//             while (fadeOutElapsed < fadeDuration) 
//             {
//                 blackOverlay.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
//                 fadeOutElapsed += Time.deltaTime;
//                 yield return null; // 等待下一帧
//             }
//             blackOverlay.alpha = 0f; // 确保最终透明
//         }
//     }

//     void ResetGame() 
//     {
//         UnityEngine.SceneManagement.SceneManager.LoadScene(
//             UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
//         );
//     }
// }



using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExitTrigger : MonoBehaviour 
{
    [Header("References")]
    public Camera mainCamera;
    public CanvasGroup blackOverlay;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float resetDelay = 0.5f;
    
    private PlayerController player; 
    private PlayerInventory inventory; // 新增：用于检查校园卡

    void Start() 
    {
        mainCamera = Camera.main;
        if (mainCamera == null) 
        {
            Debug.LogError("未找到 MainCamera！");
        }

        blackOverlay = GameObject.FindWithTag("BlackOverlay")?.GetComponent<CanvasGroup>();
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f; 
        }

        // 获取玩家组件
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerController>();
            inventory = playerObj.GetComponent<PlayerInventory>(); // 获取背包组件
        }

        if (player == null || inventory == null) 
        {
            Debug.LogError("未找到 Player 或其必要组件(PlayerController/PlayerInventory)！");
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("Player") && player.isPlayerGrounded()) 
        {
            // --- 核心逻辑修改：增加校园卡判定 ---
            if (inventory != null && inventory.hasCampusCard)
            {
                Debug.Log("<color=green>验证成功：携带校园卡，准许通过出口！</color>");
                StartCoroutine(TriggerSequenceCoroutine());
            }
            else
            {
                Debug.Log("<color=yellow>验证失败：你需要校园卡才能从这里通过！</color>");
                // 这里可以添加UI提示，比如屏幕上弹出一行字“缺少校园卡”
            }
        }
    }

    private IEnumerator TriggerSequenceCoroutine() 
    {
        // 胜利后先禁用玩家操作
        player.gameObject.SetActive(false);
        
        // 执行黑屏闪烁效果
        yield return StartCoroutine(FadeBlackOverlayCoroutine());
        
        // 延迟一小会再重启，给黑屏留点时间
        yield return new WaitForSeconds(resetDelay);
        
        ResetGame();
    }

    private IEnumerator FadeBlackOverlayCoroutine() 
    {
        Debug.Log("开始执行黑屏转场效果");
        if (blackOverlay != null) 
        {
            // 效果：瞬间变黑 -> 逐渐变透明
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