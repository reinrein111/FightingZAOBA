using UnityEngine;

public class CampusCard : MonoBehaviour
{
    public enum CardType { Girl, Boy }
    public CardType cardType = CardType.Girl;

    public float pickUpDistance = 1.0f;

    private Transform player1;
    private Transform player2;

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;
    }

    void Update()
    {
        if (player1 == null && player2 == null) return;

        float dist1 = player1 != null ? Vector2.Distance(transform.position, player1.position) : float.MaxValue;
        float dist2 = player2 != null ? Vector2.Distance(transform.position, player2.position) : float.MaxValue;
        float dist = Mathf.Min(dist1, dist2);

        if (dist < pickUpDistance)
        {
            Transform nearestPlayer = dist1 < dist2 ? player1 : player2;
            bool canPickUp = (cardType == CardType.Girl && nearestPlayer == player1) ||
                            (cardType == CardType.Boy && nearestPlayer == player2);

            if (!canPickUp)
            {
                return;
            }

            PlayerInventory inv = nearestPlayer.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                if (cardType == CardType.Girl)
                {
                    inv.hasCard_Girl = true;
                    Debug.Log($"<color=yellow>{nearestPlayer.name} 拾取了 Girl 卡！</color>");
                }
                else
                {
                    inv.hasCard_Boy = true;
                    Debug.Log($"<color=yellow>{nearestPlayer.name} 拾取了 Boy 卡！</color>");
                }
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpDistance);
    }
}
