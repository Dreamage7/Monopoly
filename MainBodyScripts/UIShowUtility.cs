using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;
    [Header("购买公共设备UI")]
    [SerializeField] GameObject utilityUiPanel;
    [SerializeField] TMP_Text utilityNameText;
    [SerializeField] TMP_Text utilityOwnerText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text oneUtilityMultipleText;
    [SerializeField] TMP_Text twoUtilityMultipleText;
    [Space]
    [SerializeField] TMP_Text mortgagedValueText;
    [Space]
    [SerializeField] Button buyUtilityButton;
    [SerializeField] Button closeUtilityButton;
    [Space]
    [SerializeField] TMP_Text utilityPriceText;
    [SerializeField] TMP_Text playerMoneyText;
    private void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityUI;
    }
    private void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityUI;
    }
    private void Start()
    {
        //utilityUiPanel.SetActive(false);
    }
    void ShowBuyUtilityUI(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        utilityNameText.text = node.name;
        if (node.Owner != null && node.Owner.name != "")
        {
            utilityOwnerText.text = node.Owner.name;
        }
        else
        {
            utilityOwnerText.text = "Null";
        }
        oneUtilityMultipleText.text = "x2";
        twoUtilityMultipleText.text = "x8";
        mortgagedValueText.text = node.MortgageValue + "$";
        utilityPriceText.text = "价格：" + node.price + "$";
        playerMoneyText.text = "资产：" + currentPlayer.ReadMoney + "$";
        if (currentPlayer.CnAffordNode(node.price) && utilityOwnerText.text == "Null")
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            buyUtilityButton.interactable = false;
        }
        utilityUiPanel.SetActive(true);
    }

    public void BuyUtilityButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyUtilityButton.interactable = false;
        utilityPriceText.text = "价格：已购买";
        playerMoneyText.text = "资产：" + playerReference.ReadMoney + "$";
        utilityOwnerText.text = playerReference.name;
    }
    public void CloseUtilityButton()
    {
        utilityUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
}
