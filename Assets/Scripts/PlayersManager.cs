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

    [Header("组件引用")]
    public RotationSystem rotationSystem;
    public SharedCameraController sharedCamera;

    void Start()
    {
        InitializePlayers();
        InitializeSystems();
    }

    private void InitializePlayers()
    {
        if (player1 == null || player2 == null)
        {
            FindPlayersInChildren();
        }

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

        if (players.Length >= 1)
        {
            player1 = players[0];
        }

        if (players.Length >= 2)
        {
            player2 = players[1];
        }
    }

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

    public PlayerController GetPlayer(int playerId)
    {
        if (playerId == 1) return player1;
        if (playerId == 2) return player2;
        return null;
    }

    public void ResetPlayers()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetLevel();
        }
    }
}
