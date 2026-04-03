using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public PlayerStatusUI statusUI; // 👈 必须在 Inspector 里拖入该玩家对应的 HUD

    [Header("物品清单状态")]
    [SerializeField] private bool _hasCard_Girl = false;
    [SerializeField] private bool _hasCard_Boy = false;

    // 当你在捡卡脚本里设置 inv.hasCard_Girl = true 时，会自动触发 UI 更新
    public bool hasCard_Girl 
    {
        get => _hasCard_Girl;
        set {
            _hasCard_Girl = value;
            if(value && statusUI != null) statusUI.ShowCard();
        }
    }

    public bool hasCard_Boy 
    {
        get => _hasCard_Boy;
        set {
            _hasCard_Boy = value;
            if(value && statusUI != null) statusUI.ShowCard();
        }
    }
}