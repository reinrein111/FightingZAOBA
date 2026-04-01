using UnityEngine;

public class CampusCard : MonoBehaviour
{
    public float pickUpDistance = 1.0f; // 拾取距离
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < pickUpDistance)
        {
            // 在玩家身上寻找“背包”或“状态记录器”
            PlayerInventory inv = player.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.hasCampusCard = true; // 拿到卡了
                Debug.Log("<color=yellow>校园卡已拾取！现在可以去开门了。</color>");
                Destroy(gameObject); // 校园卡消失
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpDistance);
    }
}