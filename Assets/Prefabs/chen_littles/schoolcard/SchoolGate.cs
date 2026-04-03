using UnityEngine;
using System.Collections;

public class SchoolGate : MonoBehaviour
{
    public enum GateType { Girl, Boy }
    public GateType gateType = GateType.Girl;

    public float openDistance = 1.5f;

    private Transform player1;
    private Transform player2;
    private bool isPlayerAtGate = false;

    public Camera girlCamera;
    public Camera boyCamera;

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;

        SetupCameras();
    }

    private void SetupCameras()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            PlayerCameraController pcc = cam.GetComponent<PlayerCameraController>();
            if (pcc != null)
            {
                if (pcc.targetPlayerId == 1)
                    girlCamera = cam;
                else if (pcc.targetPlayerId == 2)
                    boyCamera = cam;
            }
        }
    }

    void Update()
    {
        if (gateType == GateType.Girl)
        {
            CheckGateAccess(player1, inv => inv.hasCard_Girl, "女孩卡片", 1);
        }
        else
        {
            CheckGateAccess(player2, inv => inv.hasCard_Boy, "男孩卡片", 2);
        }
    }

    private void CheckGateAccess(Transform targetPlayer, System.Func<PlayerInventory, bool> cardCheck, string cardName, int playerId)
    {
        if (targetPlayer == null) return;

        float dist = Vector2.Distance(transform.position, targetPlayer.position);
        bool isAtGate = dist < openDistance;

        if (isAtGate)
        {
            PlayerInventory inv = targetPlayer.GetComponent<PlayerInventory>();

            if (inv != null && cardCheck(inv))
            {
                if (!isPlayerAtGate)
                {
                    Debug.Log($"<color=green>{cardName}验证通过！{targetPlayer.name}已到达{gameObject.name}。</color>");
                }
                isPlayerAtGate = true;

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.UpdatePlayerAtGate(playerId, true);
                }
            }
            else
            {
                isPlayerAtGate = false;
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.UpdatePlayerAtGate(playerId, false);
                }
            }
        }
        else
        {
            if (isPlayerAtGate)
            {
                Debug.Log($"<color=orange>{targetPlayer.name}离开了{gameObject.name}</color>");
            }
            isPlayerAtGate = false;
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.UpdatePlayerAtGate(playerId, false);
            }
        }
    }

    public void HidePassedPlayer(int playerId)
    {
        Transform targetPlayer = (playerId == 1) ? player1 : player2;
        if (targetPlayer != null)
        {
            targetPlayer.gameObject.SetActive(false);
            Debug.Log($"<color=orange>{targetPlayer.name}已消失</color>");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gateType == GateType.Girl ? Color.magenta : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(openDistance, openDistance, 0));
    }
}
