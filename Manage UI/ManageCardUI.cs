using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManageCardUI : MonoBehaviour
{
    [SerializeField] Image colorField;
    [SerializeField] GameObject[] buildings;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;
    [SerializeField] TMP_Text nameText;
    [SerializeField] Image iconImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;
    Player playerRefernce;
    MonopolyNode nodeRefernce;
    ManagePropertyUI propertyUIRefernce;
    public void SetCard(MonopolyNode node,Player owner,ManagePropertyUI propertySet)
    {
        nodeRefernce = node;
        playerRefernce = owner;
        propertyUIRefernce = propertySet;
        if (node.propertyColoerField != null)
        {
            colorField.color = node.propertyColoerField.color;
        }
        ShowBuildings();
        mortgageImage.SetActive(node.IsMortgaged);
        if (node.IsMortgaged)
        {
            mortgageValueText.text = "取消抵押需要：<br>" + (nodeRefernce.MortgageValue + (int)Mathf.Ceil(nodeRefernce.MortgageValue * 0.1f)) + "$";
        }
        else
        {
            mortgageValueText.text = "抵押价值：<br>" + node.MortgageValue + "$";
        }
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.interactable = node.IsMortgaged;
        nameText.text = node.name;
        switch (node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                iconImage.color = Color.green;
                break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                iconImage.color = Color.white;
                break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                iconImage.color = Color.black;
                break;
        }
    }
    public void MortgageButton()
    {
        if (!propertyUIRefernce.CheckIfMortgageAllowed())
        {
            string message = "当前区域你还有房产！无法抵押！";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (nodeRefernce.IsMortgaged)
        {
            string message = "当前区域已经抵押！";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerRefernce.CollectMoney(nodeRefernce.MortgageProperty());
        mortgageImage.SetActive(true);
        mortgageValueText.text = "取消抵押需要：<br>" + (nodeRefernce.MortgageValue + (int)Mathf.Ceil(nodeRefernce.MortgageValue * 0.1f)) + "$";
        mortgageButton.interactable = false;
        unMortgageButton.interactable = true;
        ManageUI.instance.UpdateMoneyText();
    }
    public void UnMortgageButton()
    {
        if (!nodeRefernce.IsMortgaged)
        {
            string message = "当前区域没有抵押！不用取消抵押！";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerRefernce.ReadMoney < (nodeRefernce.MortgageValue + (int)Mathf.Ceil(nodeRefernce.MortgageValue * 0.1f)))
        {
            string message = "你的资产不足已支持你取消抵押!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerRefernce.PayMoney(nodeRefernce.MortgageValue + (int)Mathf.Ceil(nodeRefernce.MortgageValue * 0.1f));
        nodeRefernce.UnMortgageProperty();
        mortgageImage.SetActive(false);
        mortgageValueText.text = "抵押价值：<br>" + nodeRefernce.MortgageValue + "$";
        mortgageButton.interactable = true;
        unMortgageButton.interactable = false;
        ManageUI.instance.UpdateMoneyText();
    }
    public void ShowBuildings()
    {
        foreach (var house in buildings)
        {
            house.SetActive(false);
        }
        if (nodeRefernce.NumberOfHouses < 5)
        {
            for (int i = 0; i < nodeRefernce.NumberOfHouses; i++)
            {
                buildings[i].SetActive(true);
            }
        }
        else
        {
            buildings[4].SetActive(true);
        }
    }
}
