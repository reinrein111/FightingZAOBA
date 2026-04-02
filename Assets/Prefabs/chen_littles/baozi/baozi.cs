using UnityEngine;

public class Baozi : MonoBehaviour
{
    [Header("设置")]
    public float eatDistance = 0.8f; // 玩家离包子多近算“吃掉”
    public int shieldAmount = 5;    // 增加的护盾层数

    public Transform player1 = null;
    public Transform player2 = null;

    void Start()
    {
        // 自动寻找玩家
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;
    }

    void Update()
    {

        // 1. 计算 2D 平面距离（忽略 Z 轴）
        float dist1 = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(player1.position.x, player1.position.y)
        );
        float dist2 = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(player2.position.x, player2.position.y)
        );

        // 2. 判定：如果进入范围
        if (dist1 < eatDistance || dist2 < eatDistance)
        {
            EatMe();
        }
    }

    void EatMe()
    {
        bool hasPlayer = false;

        if (player1 != null)
        {
            PlayerShield ps1 = player1.GetComponent<PlayerShield>();
            if (ps1 != null)
            {
                ps1.AddShield(shieldAmount);
                hasPlayer = true;
            }
        }

        if (player2 != null)
        {
            PlayerShield ps2 = player2.GetComponent<PlayerShield>();
            if (ps2 != null)
            {
                ps2.AddShield(shieldAmount);
                hasPlayer = true;
            }
        }

        if (hasPlayer)
        {
            Debug.Log("<color=cyan>【包子】已被吃掉！</color>");
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