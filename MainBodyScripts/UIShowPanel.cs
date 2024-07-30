using UnityEngine;
using UnityEngine.UI;

public class UIShowPanel : MonoBehaviour
{
    [SerializeField] GameObject humanPanel;
    [SerializeField] Button rollDiceButton;
    [SerializeField] Button endTurnButton;
    [SerializeField] Button haseChanceJailFreeCardButton;
    [SerializeField] Button haseCommunityJailFreeCardButton;

    private void OnEnable()
    {
        GameManager.OnShowHumanPanel += ShowPanel;
        MonopolyNode.OnShowHumanPanel += ShowPanel;
        CommunityChest.OnShowHumanPanel += ShowPanel;
        ChanceField.OnShowHumanPanel += ShowPanel;
        Player.OnShowHumanPanel += ShowPanel;
    }
    private void OnDisable()
    {
        GameManager.OnShowHumanPanel -= ShowPanel;
        MonopolyNode.OnShowHumanPanel -= ShowPanel;
        CommunityChest.OnShowHumanPanel -= ShowPanel;
        ChanceField.OnShowHumanPanel -= ShowPanel;
        Player.OnShowHumanPanel -= ShowPanel;
    }
    void ShowPanel(bool showPanel,bool enableRollDice,bool enableEndTurn,bool haseChanceJailFreeCard,bool haseCommunityJailFreeCard)
    {
        humanPanel.SetActive(showPanel);
        rollDiceButton.interactable = enableRollDice;
        endTurnButton.interactable = enableEndTurn;
        haseChanceJailFreeCardButton.interactable = haseChanceJailFreeCard;
        haseCommunityJailFreeCardButton.interactable = haseCommunityJailFreeCard;
    }
}
