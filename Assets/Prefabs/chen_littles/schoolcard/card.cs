using UnityEngine;

public class CampusCard : MonoBehaviour
{
    public enum CardType { Girl, Boy }
    public CardType cardType = CardType.Girl;

    public float pickUpDistance = 1.0f;

    private Transform player1;
    private Transform player2;
<<<<<<< Updated upstream
=======
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalScale;
    private bool wasPickedUp = false;
>>>>>>> Stashed changes

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;
<<<<<<< Updated upstream
=======

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        initialLocalScale = transform.localScale;
>>>>>>> Stashed changes
    }

    void Update()
    {
<<<<<<< Updated upstream
=======
        if (wasPickedUp) return;
        if (!gameObject.activeSelf) return;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                Destroy(gameObject);
=======
                wasPickedUp = true;
                gameObject.SetActive(false);
>>>>>>> Stashed changes
            }
        }
    }

    public void ResetCard()
    {
        Debug.Log($"[ResetCard] {cardType} ====== 开始 ======");
        Debug.Log($"[ResetCard] {cardType} wasPickedUp={wasPickedUp}, activeSelf={gameObject.activeSelf}, localPos={transform.localPosition}");

        wasPickedUp = false;
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        transform.localScale = initialLocalScale;

        Debug.Log($"[ResetCard] {cardType} 重置完成，调用 SetActive(true)");
        gameObject.SetActive(true);
        Debug.Log($"[ResetCard] {cardType} SetActive(true) 完成，activeSelf={gameObject.activeSelf}, worldPos={transform.position}");
    }

    void OnEnable()
    {
        Debug.Log($"<color=green>[{cardType}] OnEnable - activeSelf={gameObject.activeSelf}, wasPickedUp={wasPickedUp}</color>");
    }

    void OnDisable()
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        string stackTrace = st.ToString();
        Debug.Log($"<color=red>[{cardType}] OnDisable - activeSelf={gameObject.activeSelf}, wasPickedUp={wasPickedUp}\nStack:\n{stackTrace}</color>");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpDistance);
    }
}
