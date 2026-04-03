using UnityEngine;
using System.Collections;

public class LevelData
{
    public string levelName;
    public Vector3 player1SpawnPos;
    public Vector3 player2SpawnPos;
    public Transform worldRoot;
    public Quaternion worldRotation;
    public Vector3 worldPosition;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("关卡配置")]
    public LevelData[] levels = new LevelData[]
    {
        new LevelData()
        {
            levelName = "RootObject_Map1",
            player1SpawnPos = new Vector3(-8.01f, -9.98f, -0.33f),
            player2SpawnPos = new Vector3(-6.27f, -10.00f, -0.33f),
            worldRoot = null,
            worldRotation = Quaternion.identity,
            worldPosition = Vector3.zero
        },
        new LevelData()
        {
            levelName = "RootObject_Map2",
            player1SpawnPos = new Vector3(86.17f, -7f, -0.26f),
            player2SpawnPos = new Vector3(88.73f, -7f, -0.26f),
            worldRoot = null,
            worldRotation = Quaternion.identity,
            worldPosition = Vector3.zero
        },
        new LevelData()
        {
            levelName = "RootObject_Map3",
            player1SpawnPos = new Vector3(-109.48f, -4.24f, -0.26f),
            player2SpawnPos = new Vector3(-107.73f, -4.24f, -0.26f),
            worldRoot = null,
            worldRotation = Quaternion.identity,
            worldPosition = Vector3.zero
        }
    };

    [Header("当前状态")]
    public int currentLevelIndex = 0;
    public string currentLevelName => levels != null && currentLevelIndex < levels.Length ? levels[currentLevelIndex].levelName : "";

    private bool player1Passed = false;
    private bool player2Passed = false;
    private bool isTransitioning = false;

    private bool player1Dead = false;
    private bool player2Dead = false;

    [Header("场景切换")]
    public CanvasGroup blackOverlay;
    public float fadeDuration = 1f;

    [Header("相机引用")]
    public CameraManager cameraManager;
    public Camera playerACamera;
    public Camera playerBCamera;

    private Transform player1;
    private Transform player2;
    private GameObject thanksForPlayingUI;

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
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;

        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;

        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
        }

        if (blackOverlay == null)
        {
            GameObject overlayObj = GameObject.FindWithTag("BlackOverlay");
            if (overlayObj != null) blackOverlay = overlayObj.GetComponent<CanvasGroup>();
        }

        FindWorldRoots();
        InitializeLevel();
    }

    private void FindWorldRoots()
    {
        if (levels == null) return;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].worldRoot == null)
            {
                GameObject rootObj = GameObject.Find(levels[i].levelName);
                if (rootObj != null)
                {
                    levels[i].worldRoot = rootObj.transform;
                    levels[i].worldRotation = rootObj.transform.rotation;
                    levels[i].worldPosition = rootObj.transform.position;
                }
            }
        }
    }

    public void InitializeLevel()
    {
        if (levels == null || levels.Length == 0 || currentLevelIndex >= levels.Length) return;

        LevelData level = levels[currentLevelIndex];

        player1Passed = false;
        player2Passed = false;
        isTransitioning = false;
        player1Dead = false;
        player2Dead = false;

        ResetBothPlayers();
        RestoreAllCameras();

        RotationSystem rs = FindObjectOfType<RotationSystem>();
        if (rs != null && level.worldRoot != null)
        {
            rs.worldRoot = level.worldRoot;
        }
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

        ResetWorldPositionAndRotation();
        ResetBothPlayers();
        RestoreAllCameras();

        player1Dead = false;
        player2Dead = false;

        if (blackOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            blackOverlay.alpha = 0f;
        }
    }

    private void ResetWorldPositionAndRotation()
    {
        if (levels == null || currentLevelIndex >= levels.Length) return;
        LevelData level = levels[currentLevelIndex];

        RotationSystem rs = FindObjectOfType<RotationSystem>();
        if (rs != null && level.worldRoot != null)
        {
            level.worldRoot.rotation = level.worldRotation;
            level.worldRoot.position = level.worldPosition;
        }
    }

    private void ResetBothPlayers()
    {
        LevelData level = levels[currentLevelIndex];

        if (player1 != null)
        {
            player1.gameObject.SetActive(true);
            player1.position = level.player1SpawnPos;
            PlayerController pc1 = player1.GetComponent<PlayerController>();
            if (pc1 != null)
            {
                pc1.changeState(PlayerController.PlayerState.Grounding);
                pc1.enabled = true;
            }
        }

        if (player2 != null)
        {
            player2.gameObject.SetActive(true);
            player2.position = level.player2SpawnPos;
            PlayerController pc2 = player2.GetComponent<PlayerController>();
            if (pc2 != null)
            {
                pc2.changeState(PlayerController.PlayerState.Grounding);
                pc2.enabled = true;
            }
        }

        player1Dead = false;
        player2Dead = false;
    }

    private void RestoreAllCameras()
    {
        if (cameraManager != null)
        {
            cameraManager.SetCameraEnabled(CameraManager.CameraType.PlayerA, true);
            cameraManager.SetCameraEnabled(CameraManager.CameraType.PlayerB, true);
            cameraManager.SetupViewports();
        }
        else
        {
            if (playerACamera != null)
            {
                playerACamera.rect = new Rect(0, 0, 0.5f, 1f);
                PlayerCameraController pccA = playerACamera.GetComponent<PlayerCameraController>();
                if (pccA != null) pccA.SnapToTarget();
            }
            if (playerBCamera != null)
            {
                playerBCamera.rect = new Rect(0.5f, 0, 0.5f, 1f);
                PlayerCameraController pccB = playerBCamera.GetComponent<PlayerCameraController>();
                if (pccB != null) pccB.SnapToTarget();
            }
        }
    }

    public void PlayerPassedGate(int playerId)
    {
        if (isTransitioning) return;

        if (playerId == 1)
            player1Passed = true;
        else if (playerId == 2)
            player2Passed = true;

        if (player1Passed && player2Passed)
        {
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;

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

        if (currentLevelIndex >= levels.Length)
        {
            currentLevelIndex = levels.Length - 1;
            GameObject thanksForPlaying = GameObject.Find("ThanksForPlaying");
            if (thanksForPlaying != null)
            {
                thanksForPlaying.SetActive(true);
            }
            isTransitioning = true;
            yield break;
        }

        ResetBothPlayers();
        RestoreAllCameras();
        ResetWorldPositionAndRotation();

        player1Passed = false;
        player2Passed = false;

        if (blackOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            blackOverlay.alpha = 0f;
        }

        isTransitioning = false;
    }

    public void RespawnBothPlayers()
    {
        if (levels == null || currentLevelIndex >= levels.Length) return;

        ResetWorldPositionAndRotation();
        ResetBothPlayers();
        ResetAllCards();
        RestoreAllCameras();
    }

    public void ResetLevel()
    {
        InitializeLevel();
    }

    private void ResetAllCards()
    {
        Debug.Log("ResetAllCards");
        CampusCard[] cards = FindObjectsOfType<CampusCard>();
        foreach (CampusCard card in cards)
        {
            card.ResetCard();
        }
    }
}
