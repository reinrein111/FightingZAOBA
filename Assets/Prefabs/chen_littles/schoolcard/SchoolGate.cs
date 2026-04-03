using UnityEngine;
using System.Collections;

public class SchoolGate : MonoBehaviour
{
    public enum GateType { Girl, Boy }
    public GateType gateType = GateType.Girl;

    public float openDistance = 1.5f;

    private Transform player1;
    private Transform player2;
    private bool hasTriggered = false;

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
        if (hasTriggered) return;

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
        if (dist < openDistance)
        {
            PlayerInventory inv = targetPlayer.GetComponent<PlayerInventory>();

            if (inv != null && cardCheck(inv))
            {
                Debug.Log($"<color=green>{cardName}验证通过！{targetPlayer.name}已通过{gameObject.name}。</color>");
                hasTriggered = true;

                targetPlayer.gameObject.SetActive(false);
                Debug.Log($"<color=orange>{targetPlayer.name}已消失，相机停留在当前位置</color>");

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.PlayerPassedGate(playerId);
                }
            }
            else
            {
                Debug.Log($"{cardName}门锁着，{targetPlayer.name}需要{cardName}才能进入。");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gateType == GateType.Girl ? Color.magenta : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(openDistance, openDistance, 0));
    }
}
