using UnityEngine;

public class Baozi : MonoBehaviour
{
    [Header("设置")]
    public float eatDistance = 0.8f; // 玩家离包子多近算“吃掉”
    public int shieldAmount = 5;    // 增加的护盾层数

    private Transform player;

    void Start()
    {
        // 自动寻找玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. 计算 2D 平面距离（忽略 Z 轴）
        float dist = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(player.position.x, player.position.y)
        );

        // 2. 判定：如果进入范围
        if (dist < eatDistance)
        {
            EatMe();
        }
    }

    void EatMe()
    {
        PlayerShield ps = player.GetComponent<PlayerShield>();
        if (ps != null)
        {
            ps.AddShield(shieldAmount);
            Debug.Log("<color=cyan>【包子】已被吃掉！</color>");
            
            // 销毁包子
            Destroy(gameObject); 
        }
        else
        {
            Debug.LogWarning("玩家身上没找到 PlayerShield 脚本！");
        }
    }

    // 在 Scene 视图画一个青色的圈，方便你调整“吃”的范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, eatDistance);
    }
}