using UnityEngine;

public class SchoolGate : MonoBehaviour
{
    public enum GateType { Girl, Boy }
    public GateType gateType = GateType.Girl; // 女孩门选 Girl

    public float openDistance = 1.5f;
    private Transform player1;
    private Transform player2;
    private bool hasTriggered = false;

    void Start()
    {
        player1 = GameObject.Find("Player1")?.transform;
        player2 = GameObject.Find("Player2")?.transform;
    }
    // 在 SchoolGate.cs 脚本中添加
public void HidePassedPlayer(int playerId)
{
    // 根据 ID 找到对应的玩家物体并隐藏
    GameObject player = GameObject.Find("Player" + playerId);
    if (player != null)
    {
        player.SetActive(false);
        Debug.Log($"<color=orange>校门已手动隐藏 Player{playerId}</color>");
    }
}
    void Update()
    {
        if (hasTriggered) return;

        // 根据门类型确定要检测哪个玩家
        Transform target = (gateType == GateType.Girl) ? player1 : player2;
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist < openDistance)
        {
            PlayerInventory inv = target.GetComponent<PlayerInventory>();
            
            // 校验：有卡，且身份对应（女孩进女孩门）
            if (inv != null && inv.hasCard)
            {
                // 因为目标玩家已经是根据门类型选定的，所以这里直接判断 hasCard
                Debug.Log($"{target.name} 验证通过，进入校园！");
                hasTriggered = true;
                target.gameObject.SetActive(false);
                
                if (LevelManager.Instance != null)
                    LevelManager.Instance.PlayerPassedGate((gateType == GateType.Girl) ? 1 : 2);
            }
        }
    }
}