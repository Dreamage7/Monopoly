using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManagePropertyUI : MonoBehaviour
{
    [SerializeField] Transform cardHolder;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Button buyButton, sellButton;
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;
    Player playerplayerRefernce;
    List<MonopolyNode> nodeInSet = new List<MonopolyNode>();
    List<GameObject> cardInSet = new List<GameObject>();
    [SerializeField] GameObject buttonBox;
    public void SetProperty(List<MonopolyNode> nodes,Player owner)
    {
        nodeInSet.AddRange(nodes);
        playerplayerRefernce = owner;
        for (int i = 0; i < nodeInSet.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder,false);
            ManageCardUI manageCardUi = newCard.GetComponent<ManageCardUI>();
            manageCardUi.SetCard(nodeInSet[i],owner,this);
            cardInSet.Add(newCard);
        }
        var (list,allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(nodeInSet[0]);
        buyButton.interactable = allsame && CheckIfBuyAllowed();
        sellButton.interactable = CheckIfSwellAllowed();
        buyHousePriceText.text = "-" + nodeInSet[0].houseCost + "$";
        sellHousePriceText.text = "+" + (nodeInSet[0].houseCost / 2) + "$";
        if (nodeInSet[0].monopolyNodeType != MonopolyNodeType.Property)
        {
            buttonBox.SetActive(false);
        }
    }
    public void BuyButton()
    {
        if (!CheckIfBuyAllowed())
        {
            string message = "当前区域存在抵押！无法购买房产！";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (CheckIfBuyAllCount())
        {
            string message = "当前区域房产已达上限！无法购买房产！";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerplayerRefernce.CanAffordHouse(nodeInSet[0].houseCost))
        {
            playerplayerRefernce.BuildHousesOrHotelEvenly(nodeInSet);
            string message = "你购买了一处房产！";
            ManageUI.instance.UpdateSystemMessage(message);
            UpdateHouseVisulas();
        }
        else
        {
            string message = "你没有足够的资产！无法购买房产！";
            ManageUI.instance.UpdateSystemMessage(message);
        }
        sellButton.interactable = CheckIfSwellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }
    public void SellButton()
    {
        playerplayerRefernce.SellHouseEvenly(nodeInSet);
        UpdateHouseVisulas();
        sellButton.interactable = CheckIfSwellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }
    public bool CheckIfBuyAllCount()
    {
        if (nodeInSet.All(n => n.NumberOfHouses >= 5))
        {
            return true;
        }
        return false;
    }
    public bool CheckIfSwellAllowed()
    {
        if (nodeInSet.Any(n => n.NumberOfHouses > 0))
        {
            return true;
        }
        return false;
    }
    public bool CheckIfBuyAllowed()
    {
        if (nodeInSet.Any(n => n.IsMortgaged == true))
        {
            return false;
        }
        return true;
    }
    public bool CheckIfMortgageAllowed()
    {
        if (nodeInSet.Any(n => n.NumberOfHouses > 0))
        {
            return false;
        }
        return true;
    }
    void UpdateHouseVisulas()
    {
        foreach (var card in cardInSet)
        {
            card.GetComponent<ManageCardUI>().ShowBuildings();
        }
    }
}
