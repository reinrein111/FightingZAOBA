using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("关卡场景配置")]
    public string[] levelScenes = new string[]
    {
        SceneNames.MAP_0,
        SceneNames.MAP_1,
        SceneNames.MAP_2
    };

    [Header("当前状态")]
    public int currentLevelIndex = 0;

    public bool player1Passed = false;
    public bool player2Passed = false;
    private bool isTransitioning = false;

    private bool player1Dead = false;
    private bool player2Dead = false;

    [Header("场景切换")]
    public CanvasGroup blackOverlay;
    public float fadeDuration = 1f;

    [Header("玩家引用")]
    public Transform player1;
    public Transform player2;

    [Header("门引用")]
    public SchoolGate girlGate;
    public SchoolGate boyGate;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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

    public void HandlePlayerDeath(PlayerController player)
    {
        if (player.playerId == 1)
            player1Dead = true;
        else
            player2Dead = true;

        if (player1Dead && player2Dead)
        {
            StartCoroutine(ResetLevelSequence());
        }
    }

    private IEnumerator ResetLevelSequence()
    {
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            blackOverlay.alpha = 1f;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdatePlayerAtGate(int playerId, bool isAtGate)
    {
        if (isTransitioning) return;

        if (playerId == 1)
            player1Passed = isAtGate;
        else if (playerId == 2)
            player2Passed = isAtGate;

        Debug.Log($"<color=cyan>玩家{playerId}在门位置：{isAtGate}，当前状态: P1={player1Passed}, P2={player2Passed}</color>");

        if (player1Passed && player2Passed)
        {
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;

        if (girlGate != null) girlGate.HidePassedPlayer(1);
        if (boyGate != null) boyGate.HidePassedPlayer(2);

        Debug.Log("<color=yellow>两人都已通过，准备切换场景...</color>");

        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            blackOverlay.alpha = 1f;
        }

        currentLevelIndex++;

        if (currentLevelIndex >= levelScenes.Length)
        {
            Debug.Log("<color=green>所有关卡已完成，返回主菜单</color>");
            SceneManager.LoadScene(SceneNames.MAIN_MENU);
            yield break;
        }

        string nextScene = levelScenes[currentLevelIndex];
        Debug.Log($"<color=green>正在加载场景: {nextScene}</color>");

        SceneManager.LoadScene(nextScene);
    }

    public void RespawnBothPlayers()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetLevel()
    {
        InitializeLevel();
    }

    public void SkipToNextLevel()
    {
        isTransitioning = false;
        StopAllCoroutines();
        StartCoroutine(TransitionToNextLevel());
    } 
}
