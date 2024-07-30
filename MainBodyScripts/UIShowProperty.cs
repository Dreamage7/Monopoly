using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShowProperty : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;
    [Header("购买房子UI")]
    [SerializeField] GameObject propertyUiPanel;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] TMP_Text propertyOwnerText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text rentPriceText;
    [SerializeField] TMP_Text oneHouseRentText;
    [SerializeField] TMP_Text twoHouseRentText;
    [SerializeField] TMP_Text threeHouseRentText;
    [SerializeField] TMP_Text fourHouseRentText;
    [SerializeField] TMP_Text hotelRentText;
    [Space]
    [SerializeField] TMP_Text houseAndHotelPriceText;
    [SerializeField] TMP_Text mortgagedValueText;
    [Space]
    [SerializeField] Button buyPropertyButton;
    [SerializeField] Button closePropertyButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;
    private void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUI;
    }
    private void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUI;
    }
    private void Start()
    {
        //propertyUiPanel.SetActive(false);
    }
    void ShowBuyPropertyUI(MonopolyNode node,Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        propertyNameText.text = node.name;
        if (node.Owner != null && node.Owner.name != "")
        {
            propertyOwnerText.text = node.Owner.name;
        }
        else
        {
            propertyOwnerText.text = "Null";
        }
        colorField.color = node.propertyColoerField.color;
        rentPriceText.text = node.baseRent + "$";
        oneHouseRentText.text = node.rentwithHouses[0] + "$";
        twoHouseRentText.text = node.rentwithHouses[1] + "$";
        threeHouseRentText.text = node.rentwithHouses[2] + "$";
        fourHouseRentText.text = node.rentwithHouses[3] + "$";
        hotelRentText.text = node.rentwithHouses[4] + "$";
        houseAndHotelPriceText.text = node.houseCost + "$";
        mortgagedValueText.text = node.MortgageValue + "$";
        propertyPriceText.text = "价格：" + node.price + "$";
        playerMoneyText.text = "资产：" + currentPlayer.ReadMoney + "$";
        if (currentPlayer.CnAffordNode(node.price) && propertyOwnerText.text == "Null")
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            buyPropertyButton.interactable = false;
        }
        propertyUiPanel.SetActive(true);
    }

    public void BuyPropertyButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyPropertyButton.interactable = false;
        propertyPriceText.text = "价格：已购买";
        playerMoneyText.text = "资产：" + playerReference.ReadMoney + "$";
        propertyOwnerText.text = playerReference.name;
    }
    public void ClosePropertyButton()
    {
        propertyUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
}
