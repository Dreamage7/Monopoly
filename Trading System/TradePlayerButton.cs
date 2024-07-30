using TMPro;
using UnityEngine;

public class TradePlayerButton : MonoBehaviour
{
    Player playerReference;
    [SerializeField] TMP_Text playerName;
    public void SetPlayer(Player player)
    {
        playerReference = player;
        playerName.text = player.name;
    }
    public void SelectPlayer()
    {
        MaybeTradingSystem.instance.ShowRightPlayer(playerReference);
    }
}
