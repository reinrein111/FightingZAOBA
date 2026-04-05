// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System.Collections;

// public class LevelManager : MonoBehaviour
// {
//     public static LevelManager Instance { get; private set; }

//     [Header("关卡场景配置")]
//     public string[] levelScenes = new string[]
//     {
//         SceneNames.MAP_0,
//         SceneNames.MAP_1,
//         SceneNames.MAP_2
//     };

//     [Header("当前状态")]
//     public int currentLevelIndex = 0;

//     public bool player1Passed = false;
//     public bool player2Passed = false;
//     private bool isTransitioning = false;

//     private bool player1Dead = false;
//     private bool player2Dead = false;

//     [Header("场景切换")]
//     public CanvasGroup blackOverlay;
//     public float fadeDuration = 1f;

//     [Header("玩家引用")]
//     public Transform player1;
//     public Transform player2;

//     [Header("门引用")]
//     public SchoolGate girlGate;
//     public SchoolGate boyGate;

//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//     }

//     void Start()
//     {
//         InitializeLevel();
//     }

//     public void InitializeLevel()
//     {
//         player1Passed = false;
//         player2Passed = false;
//         isTransitioning = false;
//         player1Dead = false;
//         player2Dead = false;
//     }

//     public void HandlePlayerDeath(PlayerController player)
//     {
//         if (player.playerId == 1)
//             player1Dead = true;
//         else
//             player2Dead = true;

//         if (player1Dead && player2Dead)
//         {
//             StartCoroutine(ResetLevelSequence());
//         }
//     }

//     private IEnumerator ResetLevelSequence()
//     {
//         if (blackOverlay != null)
//         {
//             blackOverlay.alpha = 0f;
//             float elapsed = 0f;
//             while (elapsed < fadeDuration)
//             {
//                 elapsed += Time.deltaTime;
//                 blackOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
//                 yield return null;
//             }
//             blackOverlay.alpha = 1f;
//         }

//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//     }

//     public void UpdatePlayerAtGate(int playerId, bool isAtGate)
//     {
//         if (isTransitioning) return;

//         if (playerId == 1)
//             player1Passed = isAtGate;
//         else if (playerId == 2)
//             player2Passed = isAtGate;

//         Debug.Log($"<color=cyan>玩家{playerId}在门位置：{isAtGate}，当前状态: P1={player1Passed}, P2={player2Passed}</color>");

//         if (player1Passed && player2Passed)
//         {
//             StartCoroutine(TransitionToNextLevel());
//         }
//     }

//     private IEnumerator TransitionToNextLevel()
//     {
//         isTransitioning = true;

//         if (girlGate != null) girlGate.HidePassedPlayer(1);
//         if (boyGate != null) boyGate.HidePassedPlayer(2);

//         Debug.Log("<color=yellow>两人都已通过，准备切换场景...</color>");

//         if (blackOverlay != null)
//         {
//             blackOverlay.alpha = 0f;
//             float elapsed = 0f;
//             while (elapsed < fadeDuration)
//             {
//                 elapsed += Time.deltaTime;
//                 blackOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
//                 yield return null;
//             }
//             blackOverlay.alpha = 1f;
//         }

//         currentLevelIndex++;

//         if (currentLevelIndex >= levelScenes.Length)
//         {
//             Debug.Log("<color=green>所有关卡已完成，返回主菜单</color>");
//             SceneManager.LoadScene(SceneNames.MAIN_MENU);
//             yield break;
//         }

//         string nextScene = levelScenes[currentLevelIndex];
//         Debug.Log($"<color=green>正在加载场景: {nextScene}</color>");

//         SceneManager.LoadScene(nextScene);
//     }

//     public void RespawnBothPlayers()
//     {
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//     }

//     public void ResetLevel()
//     {
//         InitializeLevel();
//     }

//     public void SkipToNextLevel()
//     {
//         isTransitioning = false;
//         StopAllCoroutines();
//         StartCoroutine(TransitionToNextLevel());
//     } 
// }


using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("关卡场景配置")]
    public string[] levelScenes = new string[]
    {
        "Map0", // 请确保这些名字与你的场景名一致
        "Map1",
        "Map2"
    };

    [Header("当前状态")]
    public int currentLevelIndex = 0;
    public bool player1Passed = false;
    public bool player2Passed = false;
    private bool isTransitioning = false;

    private bool player1Dead = false;
    private bool player2Dead = false;

    [Header("场景切换UI")]
    public CanvasGroup blackOverlay;
    public float fadeDuration = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 如果你希望关卡管理器跨场景存在，取消下面这行的注释
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeLevel();
    }

    public void InitializeLevel()
    {
        player1Passed = false;
        player2Passed = false;
        isTransitioning = false;
        player1Dead = false;
        player2Dead = false;
    }

    // --- 修复报错 1：增加 SchoolGate 调用的接口 ---
    public void PlayerPassedGate(int playerId)
    {
        if (isTransitioning) return;

        if (playerId == 1) player1Passed = true;
        else if (playerId == 2) player2Passed = true;

        Debug.Log($"<color=cyan>玩家{playerId}通关。当前状态: P1={player1Passed}, P2={player2Passed}</color>");

        // 判定：双人都进门了才切关
        if (player1Passed && player2Passed)
        {
            StartCoroutine(TransitionToNextLevel());
        }
    }

    // --- 修复报错 2：增加隐藏玩家的逻辑 ---
    public void HidePassedPlayer(int playerId)
    {
        GameObject p = GameObject.Find("Player" + playerId);
        if (p != null)
        {
            p.SetActive(false);
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // 切换前可以再次确保隐藏
        HidePassedPlayer(1);
        HidePassedPlayer(2);

        Debug.Log("<color=yellow>双人集结完毕，正在切换关卡...</color>");

        // 黑屏过渡
        if (blackOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
        }

        currentLevelIndex++;

        if (currentLevelIndex >= levelScenes.Length)
        {
            Debug.Log("<color=green>通关！回到主菜单</color>");
            SceneManager.LoadScene("MainMenu"); // 确保你有这个场景
        }
        else
        {
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
    }

    // 在 LevelManager.cs 中添加这个函数
public void RespawnBothPlayers()
{
    Debug.Log("<color=red>[LevelManager] 触发地刺/陷阱，全员重置当前关卡！</color>");
    
    // 停止所有正在进行的协程（防止切关和重置冲突）
    StopAllCoroutines();
    
    // 重新加载当前活动的场景
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
    // 其他原有逻辑...
    public void HandlePlayerDeath(PlayerController player)
    {
        if (player.playerId == 1) player1Dead = true;
        else player2Dead = true;

        if (player1Dead && player2Dead) StartCoroutine(ResetLevelSequence());
    }
// 在 LevelManager.cs 中添加这个方法
public void ResetLevel()
{
    Debug.Log("<color=orange>[LevelManager] 收到 PlayersManager 的指令：重置关卡状态</color>");
    
    // 1. 调用现有的初始化逻辑（清空过门状态、死亡状态等）
    InitializeLevel();

    // 2. 如果你的重置逻辑是“重新加载当前场景”，可以加上这一行：
    // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
    private IEnumerator ResetLevelSequence()
    {
        // 死亡黑屏重置逻辑...
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return null;
    }
}