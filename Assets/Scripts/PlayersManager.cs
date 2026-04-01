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
    
    [Header("起始位置")]
    public Vector3 player1StartPos = new Vector3(-4f, -7.76f, -6.91f);
    public Vector3 player2StartPos = new Vector3(-2f, -7.76f, -6.91f);
    
    [Header("组件引用")]
    public RotationSystem rotationSystem;
    public SharedCameraController sharedCamera;
    
    void Start()
    {
        InitializePlayers();
        InitializeSystems();
    }
    
    /// <summary>
    /// 初始化两个玩家的位置
    /// </summary>
    private void InitializePlayers()
    {
        // 如果没有指定玩家，尝试在子对象中查找
        if (player1 == null || player2 == null)
        {
            FindPlayersInChildren();
        }
        
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
        
        if (players.Length >= 1)
        {
            player1 = players[0];
        }
        
        if (players.Length >= 2)
        {
            player2 = players[1];
        }
    }
    
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
        
        // 查找或创建SharedCamera
        if (sharedCamera == null)
        {
            sharedCamera = FindObjectOfType<SharedCameraController>();
            if (sharedCamera == null)
            {
                // 尝试在主相机上添加
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    sharedCamera = mainCam.gameObject.AddComponent<SharedCameraController>();
                }
            }
        }
        
        // 设置相机跟随
        if (sharedCamera != null && player1 != null && player2 != null)
        {
            sharedCamera.SetPlayers(player1.transform, player2.transform);
            sharedCamera.SnapToTarget();
        }
    }
    
    /// <summary>
    /// 获取指定ID的玩家
    /// </summary>
    public PlayerController GetPlayer(int playerId)
    {
        if (playerId == 1) return player1;
        if (playerId == 2) return player2;
        return null;
    }
    
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
        }
    }
}
