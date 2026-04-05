using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public PlayerStatusUI statusUI; // 拖入该玩家对应的 HUD

    public enum PlayerIdentity { Girl, Boy }
    [Header("身份设置")]
    public PlayerIdentity myIdentity; // Player1 选 Girl，Player2 选 Boy

    [SerializeField] private bool _hasCard = false;

    public bool hasCard 
    {
        get => _hasCard;
        set {
            _hasCard = value;
            if(value && statusUI != null) statusUI.ShowCard();
        }
    }
}