using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;
    [Header("购买地铁UI")]
    [SerializeField] GameObject railroadUiPanel;
    [SerializeField] TMP_Text railroadNameText;
    [SerializeField] TMP_Text railroadOwnerText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text oneRailroadRentText;
    [SerializeField] TMP_Text twoRailroadRentText;
    [SerializeField] TMP_Text threeRailroadRentText;
    [SerializeField] TMP_Text fourRailroadRentText;
    [Space]
    [SerializeField] TMP_Text mortgagedValueText;
    [Space]
    [SerializeField] Button buyRailroadButton;
    [SerializeField] Button closeRailroadButton;
    [Space]
    [SerializeField] TMP_Text railroadPriceText;
    [SerializeField] TMP_Text playerMoneyText;
    private void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowBuyRailroadUI;
    }
    private void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowBuyRailroadUI;
    }
    private void Start()
    {
        //railroadUiPanel.SetActive(false);
    }
    void ShowBuyRailroadUI(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        railroadNameText.text = node.name;
        if (node.Owner != null && node.Owner.name != "")
        {
            railroadOwnerText.text = node.Owner.name;
        }
        else
        {
            railroadOwnerText.text = "Null";
        }
        oneRailroadRentText.text = node.baseRent + "$";
        twoRailroadRentText.text = node.baseRent * 2 + "$";
        threeRailroadRentText.text = node.baseRent * 4 + "$";
        fourRailroadRentText.text = node.baseRent * 8 + "$";
        mortgagedValueText.text = node.MortgageValue + "$";
        railroadPriceText.text = "价格：" + node.price + "$";
        playerMoneyText.text = "资产：" + currentPlayer.ReadMoney + "$";
        if (currentPlayer.CnAffordNode(node.price) && railroadOwnerText.text == "Null")
        {
            buyRailroadButton.interactable = true;
        }
        else
        {
            buyRailroadButton.interactable = false;
        }
        railroadUiPanel.SetActive(true);
    }

    public void BuyRailroadButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyRailroadButton.interactable = false;
        railroadPriceText.text = "价格：已购买";
        playerMoneyText.text = "资产：" + playerReference.ReadMoney + "$";
        railroadOwnerText.text = playerReference.name;
    }
    public void CloseRailroadButton()
    {
        railroadUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
}
