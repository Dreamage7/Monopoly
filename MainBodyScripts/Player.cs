using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Player
{
    public enum PlayerType
    {
        HUMAN,
        AI
    }
    //玩家
    public PlayerType playerType;
    public string name;
    int money;
    MonopolyNode currentNode; //当前位置
    bool isInJail; //是否进监狱
    int numTurnsInJail; //在监狱待多久了,计时器
    [SerializeField] GameObject myToken; //玩家代币
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();
    bool haseChanceJailFreeCard, haseCommunityJailFreeCard;
    public bool HaseChanceJailFreeCard => haseChanceJailFreeCard;
    public bool HaseCommunityJailFreeCard => haseCommunityJailFreeCard;
    public List<MonopolyNode> GetMyMonopolyNodes => myMonopolyNodes;
    //玩家信息输入
    PlayerInfo myInfo;
    public PlayerInfo MyInfo => myInfo;

    //AI
    int aiMoneySavity = 100;

    //AI状态
    public enum AiStates
    { 
        Idle,
        TradingState
    }
    public AiStates aiStates;

    //获取玩家信息
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentNode;
    public int ReadMoney => money;
    //信息
    public delegate void UpdateMassage(string _maggage,Player currentPlayer);
    public static UpdateMassage OnUpdateMassage;
    //人类面板
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool haseChanceJailFreeCard, bool haseCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;
    public void Inititialize(MonopolyNode startNode,int startMoney,PlayerInfo info,GameObject token,Color color)
    {
        currentNode = startNode;
        money = startMoney;
        myInfo = info;
        myToken = token;
        info.SetPlayerAndCash(name,money);
        info.SetArrow(color);
        ActivateSelector(false);
    }

    public void SetMyCurrentNode(MonopolyNode newNode)
    {
        currentNode = newNode;
        //玩家到了当前位置
        newNode.PlayerLandedOnNode(this);
        //是否可以建筑房屋
        if (playerType == PlayerType.AI)
        {
            CheckIfPlayerHasASet();
            UnMortgageProperties();
        }
    }

    public void CollectMoney(int amount) //加钱
    {
        money += amount;
        myInfo.SetPlayerCash(money);
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0 || !GameManager.instance.HasRolledDice && ReadMoney >= 0;
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn,HaseChanceJailFreeCard, HaseCommunityJailFreeCard);
        }
    }
    public void ReducedMoney(int amount) //减钱
    {
        money -= amount;
        myInfo.SetPlayerCash(money);
    }

    internal bool CnAffordNode(int price)
    {
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {
        money -= node.price;
        node.SetOwner(this);
        myInfo.SetPlayerCash(money);
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }

    void SortPropertiesByPrice()
    {
        myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount,Player owner) //交房租
    {
        //有没有足够的钱
        if (money < rentAmount)
        {
            //资金不足
            if (playerType == PlayerType.AI)
            {
                HandleInsufficientFunds(rentAmount);
            }
            else
            {
                OnShowHumanPanel.Invoke(true, false, false, HaseChanceJailFreeCard, HaseCommunityJailFreeCard);
            }
        }
        ReducedMoney(rentAmount);
        owner.CollectMoney(rentAmount);
    }

    internal void PayMoney(int amount) //交税
    {
        //有没有足够的钱
        if (money < amount)
        {
            //资金不足
            if (playerType == PlayerType.AI)
            {
                HandleInsufficientFunds(amount);
            }
        }
        ReducedMoney(amount);
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0 || !GameManager.instance.HasRolledDice && ReadMoney >= 0;
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, HaseChanceJailFreeCard, HaseCommunityJailFreeCard);
        }
    }

    //------------------------------------监狱------------------------------------------
    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        //定位
        MonopolyBoard.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }

    public void SetOutOfJail()
    {
        isInJail = false;
        numTurnsInJail = 0;
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfJail = 10;
        if (indexOnBoard > indexOfJail)
        {
            result = indexOfJail - indexOnBoard;
        }
        else
        {
            result = indexOfJail - indexOnBoard;
        }
        return result;
    }

    public int NumTurnsInJail => numTurnsInJail;

    public void IncreaseNumTurnsInJail()
    {
        numTurnsInJail++;
    }

    //--------------------------------关于街道维修----------------------------------------
    public int[] CountHousesAndHotels()
    {
        int houses = 0;//索引0
        int hotels = 0;//索引1

        foreach (var node in myMonopolyNodes)
        {
            if (node.NumberOfHouses != 5)
            {
                houses += node.NumberOfHouses;
            }
            else
            {
                hotels += 1;
            }
        }

        int[] allBuilding = new int[] {houses,hotels};
        return allBuilding;
    }
    //--------------------------------检查玩家的房子--------------------------------------
    void CheckIfPlayerHasASet()
    {
        List<MonopolyNode> processSet = null;

        foreach (var node in myMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            if (!allSame)
            {
                continue;
            }
            List<MonopolyNode> nodeSet = list;
            if (nodeSet != null && nodeSet != processSet)
            {
                bool hasMordgadedNode = nodeSet.Any(node => node.IsMortgaged)?true:false;
                if (!hasMordgadedNode)
                {
                    if (nodeSet[0].monopolyNodeType == MonopolyNodeType.Property)
                    {
                        //我们可以建房子
                        BuildHousesOrHotelEvenly(nodeSet);
                        processSet = nodeSet;
                    }
                }
            }
        }
    }
    //------------------------------------建房子-----------------------------------------
    internal void BuildHousesOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
    {
        int minHouses = int.MaxValue;
        int maxHouses = int.MinValue;
        foreach (var node in nodesToBuildOn)
        {
            int numOfHouses = node.NumberOfHouses;
            if (numOfHouses < minHouses)
            {
                minHouses = numOfHouses;
            }
            if (numOfHouses > maxHouses && numOfHouses < 5)
            {
                maxHouses = numOfHouses;
            }
        }
        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses<5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHousesOrHotel();
                PayMoney(node.houseCost);
                break;
            }
        }
    }
    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)
    {
        int minHouses = int.MaxValue;
        bool housesold = false;
        foreach (var node in nodesToSellFrom)
        {
            minHouses = Mathf.Min(minHouses,node.NumberOfHouses);
        }
        for (int i = nodesToSellFrom.Count-1; i >= 0 ; i--)
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouses)
            {
                CollectMoney(nodesToSellFrom[i].SellHousesOrHotel());
                housesold = true;
                break;
            }
        }
        if (!housesold)
        {
            CollectMoney(nodesToSellFrom[nodesToSellFrom.Count-1].SellHousesOrHotel());
        }
    }
    //------------------------------------资金不足----------------------------------------
    public void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell = 0;
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }
        while (money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHousesOrHotel());
                    allHouses--;
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (!node.IsMortgaged)?1:0;
        }
        while (money < amountToPay && allPropertiesToMortgage > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (!node.IsMortgaged) ? 1 : 0;
                if (propertiesToMortgage >0)
                {
                    CollectMoney(node.MortgageProperty());
                    allPropertiesToMortgage--;
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        //破产
        if (playerType == PlayerType.AI)
        {
            Bankrupt();
        }
    }
    internal void Bankrupt()
    {

        OnUpdateMassage.Invoke(name +"<color=red>破产了</color>",this);
        for (int i = myMonopolyNodes.Count-1; i >= 0 ; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }
        if (haseChanceJailFreeCard)
        {
            ChanceField.instance.AddBackJailFreeCard();
        }
        if (haseCommunityJailFreeCard)
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }
        ActivateSelector(false);
        myInfo.SetPlayerBankrupt();
        GameManager.instance.RemovePlayer(this);
    }
    internal void Bankrupt(int num)
    {
        for (int i = myMonopolyNodes.Count - 1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }
        if (haseChanceJailFreeCard)
        {
            ChanceField.instance.AddBackJailFreeCard();
        }
        if (haseCommunityJailFreeCard)
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }
    }
    void UnMortgageProperties()
    {
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)
            {
                int cost = node.MortgageValue + (int)Mathf.Ceil(node.MortgageValue * 0.1f);
                if (money >= aiMoneySavity + cost)
                {
                    PayMoney(cost);
                    node.UnMortgageProperty();
                }
            }
        }
    }
    public bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI)
        {
            return (money-aiMoneySavity) >= price;
        }
        return money >= price;
    }
    public void ActivateSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }
    public void AddProperty(MonopolyNode node)
    {
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }
    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
        SortPropertiesByPrice();
    }
    public void ChangeState(AiStates state)
    {
        if (playerType == PlayerType.HUMAN)
        {
            return;
        }
        aiStates = state;
        switch(aiStates)
        {
            case AiStates.Idle:
                {
                    GameManager.instance.Continue();
                }
                break;
            case AiStates.TradingState:
                {
                    MaybeTradingSystem.instance.FindMissingProperty(this);
                }
                break;
        }
    }
    public void AddChanceJailFreeCard()
    {
        haseChanceJailFreeCard = true;
    }
    public void AddCommunityJailFreeCard()
    {
        haseCommunityJailFreeCard = true;
    }
    public void UseChanceJailFreeCard()
    {
        if (!isInJail)
        {
            return;
        }
        haseChanceJailFreeCard = false;
        ChanceField.instance.AddBackJailFreeCard();
        SetOutOfJail();
        OnUpdateMassage.Invoke(name + "使用了免狱卡",this);
        bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
        bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0 || !GameManager.instance.HasRolledDice && ReadMoney >= 0;
        OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, HaseChanceJailFreeCard, HaseCommunityJailFreeCard);
    }
    public void UseCommunityJailFreeCard()
    {
        if (!isInJail)
        {
            return;
        }
        haseCommunityJailFreeCard = false;
        CommunityChest.instance.AddBackJailFreeCard();
        SetOutOfJail();
        OnUpdateMassage.Invoke(name + "使用了免狱卡", this);
        bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
        bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0 || !GameManager.instance.HasRolledDice && ReadMoney >= 0;
        OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, HaseChanceJailFreeCard, HaseCommunityJailFreeCard);
    }
}
