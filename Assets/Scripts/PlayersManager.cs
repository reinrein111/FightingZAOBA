/**
 * @file PlayersManager.cs
 * @brief 双人游戏玩家管理器 - 管理两个玩家的初始化和引用
 * @author ZHY
 * @version 3.0
 * @time 26-4-1
 */
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    [Header("玩家引用")]
    public PlayerController player1;
    public PlayerController player2;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
    [Header("起始位置")]
    public Vector3 player1StartPos = new Vector3(-6.27f, -10.07f, -6.91f);
    public Vector3 player2StartPos = new Vector3(-4.73f, -10.07f, -6.91f);
    
    [Header("组件引用")]
    public RotationSystem rotationSystem;
    public SharedCameraController sharedCamera;
    
=======
=======
>>>>>>> Stashed changes

    [Header("组件引用")]
    public RotationSystem rotationSystem;
    public SharedCameraController sharedCamera;

<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    void Start()
    {
        InitializePlayers();
        InitializeSystems();
    }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
    /// <summary>
    /// 初始化两个玩家的位置
    /// </summary>
    private void InitializePlayers()
    {
        // 如果没有指定玩家，尝试在子对象中查找
=======

    private void InitializePlayers()
    {
>>>>>>> Stashed changes
=======

    private void InitializePlayers()
    {
>>>>>>> Stashed changes
        if (player1 == null || player2 == null)
        {
            FindPlayersInChildren();
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        
        // 设置玩家ID和起始位置
        if (player1 != null)
        {
            player1.playerId = 1;
            player1.transform.position = player1StartPos;
        }
        
        if (player2 != null)
        {
            player2.playerId = 2;
            player2.transform.position = player2StartPos;
        }
    }
    
    /// <summary>
    /// 在子对象中查找玩家
    /// </summary>
    private void FindPlayersInChildren()
    {
        PlayerController[] players = GetComponentsInChildren<PlayerController>();
        
=======
=======
>>>>>>> Stashed changes

        if (LevelManager.Instance != null && LevelManager.Instance.levels != null)
        {
            var level = LevelManager.Instance.levels[LevelManager.Instance.currentLevelIndex];
            if (level != null)
            {
                if (player1 != null)
                {
                    player1.playerId = 1;
                    player1.transform.position = level.player1SpawnPos;
                }

                if (player2 != null)
                {
                    player2.playerId = 2;
                    player2.transform.position = level.player2SpawnPos;
                }

                if (rotationSystem != null && level.worldRoot != null)
                {
                    rotationSystem.worldRoot = level.worldRoot;
                }
            }
        }
        else
        {
            if (player1 != null)
            {
                player1.playerId = 1;
            }

            if (player2 != null)
            {
                player2.playerId = 2;
            }
        }
    }

    private void FindPlayersInChildren()
    {
        PlayerController[] players = GetComponentsInChildren<PlayerController>();

<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        if (players.Length >= 1)
        {
            player1 = players[0];
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        
=======

>>>>>>> Stashed changes
=======

>>>>>>> Stashed changes
        if (players.Length >= 2)
        {
            player2 = players[1];
        }
    }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
    /// <summary>
    /// 初始化系统引用，包括RotationSystem和SharedCameraController（两个玩家的引用）
    /// </summary>
    private void InitializeSystems()
    {
        // 查找或创建RotationSystem
        if (rotationSystem == null)
        {
            rotationSystem = FindObjectOfType<RotationSystem>();
            if (rotationSystem == null)
            {
                GameObject go = new GameObject("RotationSystem");
                rotationSystem = go.AddComponent<RotationSystem>();
            }
        }
    
        // 查找SharedCamera
        sharedCamera = FindObjectOfType<SharedCameraController>();
        Camera mainCam = Camera.main;
        sharedCamera.SetPlayers(player1.transform, player2.transform);
        sharedCamera.SnapToTarget();
    }
    
    /// <summary>
    /// 获取指定ID的玩家
    /// </summary>
=======
=======
>>>>>>> Stashed changes

    private void InitializeSystems()
    {
        if (rotationSystem == null)
        {
            rotationSystem = FindObjectOfType<RotationSystem>();
        }

        sharedCamera = FindObjectOfType<SharedCameraController>();
        if (sharedCamera != null && player1 != null && player2 != null)
        {
            sharedCamera.SetPlayers(player1.transform, player2.transform);
            sharedCamera.SnapToTarget();
        }
    }

<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    public PlayerController GetPlayer(int playerId)
    {
        if (playerId == 1) return player1;
        if (playerId == 2) return player2;
        return null;
    }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
    /// <summary>
    /// 重置两个玩家到起始位置
    /// </summary>
    public void ResetPlayers()
    {
        if (player1 != null)
        {
            player1.transform.position = player1StartPos;
            player1.changeState(PlayerController.PlayerState.Grounding);
        }
        
        if (player2 != null)
        {
            player2.transform.position = player2StartPos;
            player2.changeState(PlayerController.PlayerState.Grounding);
        }
        
        // 重置相机
        if (sharedCamera != null)
        {
            sharedCamera.SnapToTarget();
=======
=======
>>>>>>> Stashed changes

    public void ResetPlayers()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetLevel();
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        }
    }
}
