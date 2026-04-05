using UnityEngine;

public class CampusCard : MonoBehaviour
{
    public enum CardType { Girl, Boy }
    public CardType cardType = CardType.Girl;

    public float pickUpDistance = 1.0f;

    private Transform player1;
    private Transform player2;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalScale;
    private bool wasPickedUp = false;

    void Start()
    {
        player1 = GameObject.Find("Player1")?.transform;
        player2 = GameObject.Find("Player2")?.transform;

        // 记录初始位置，用于重置
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        initialLocalScale = transform.localScale;
    }

    void Update()
    {
        if (wasPickedUp || !gameObject.activeSelf) return;

        float dist1 = player1 != null ? Vector2.Distance(transform.position, player1.position) : float.MaxValue;
        float dist2 = player2 != null ? Vector2.Distance(transform.position, player2.position) : float.MaxValue;
        
        float minDist = Mathf.Min(dist1, dist2);
        Transform nearestPlayer = dist1 < dist2 ? player1 : player2;

        if (minDist < pickUpDistance)
        {
            PlayerInventory inv = nearestPlayer.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                // 身份匹配检查
                bool isMatch = (cardType == CardType.Girl && inv.myIdentity == PlayerInventory.PlayerIdentity.Girl) ||
                               (cardType == CardType.Boy && inv.myIdentity == PlayerInventory.PlayerIdentity.Boy);

                if (isMatch)
                {
                    inv.hasCard = true; // 捡起，触发 UI
                    wasPickedUp = true;
                    gameObject.SetActive(false);
                    Debug.Log($"<color=yellow>{nearestPlayer.name} 拾取了 {cardType} 卡！</color>");
                }
            }
        }
    }

    // 👈 修复报错：LevelManager 需要这个方法
    public void ResetCard()
    {
        wasPickedUp = false;
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        transform.localScale = initialLocalScale;
        gameObject.SetActive(true);
        Debug.Log($"{cardType} 卡片已重置");
    }
}