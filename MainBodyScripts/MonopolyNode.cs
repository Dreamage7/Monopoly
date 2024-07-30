using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public enum MonopolyNodeType
{ 
    Null,
    Property,
    Railroad,
    Utility,
    Chance,
    CommuntiyChest,
    Tax,
    Go,
    Jail,
    GoToJail,
    FreeParking
}
public class MonopolyNode : MonoBehaviour
{
    public MonopolyNodeType monopolyNodeType;
    public Image propertyColoerField;
    [Header("房产名称")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;
    [Header("房产价格")]
    public int price; //价格
    public int houseCost;//房子建造价格
    [SerializeField] TMP_Text priceText;
    [Header("房产租金")]
    [SerializeField] bool calculateRentAuto; //自动计算租金
    [SerializeField] int currentRent; //当前租金
    [SerializeField] internal int baseRent; //基本租金
    [SerializeField] internal List<int> rentwithHouses = new List<int>(); //根据拥有数量的房租
    int numberOfHouses; //拥有数量
    [Header("房子游戏物体")]
    [SerializeField] GameObject[] houses;//房子
    [SerializeField] GameObject hotel;//酒店
    [Header("房产抵押")]
    [SerializeField] GameObject mortgageImage; //抵押贷款图片
    [SerializeField] GameObject propertyImage; //房产图片
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue; //抵押价值
    [Header("房产所有者")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;
    public Player Owner => owner;
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerUpdate();
    }
    //信息
    public delegate void UpdateMassage(string _maggage,Player currentPlayer);
    public static UpdateMassage OnUpdateMassage;
    //社区卡
    public delegate void DrawCommunityCard(Player player,int quantity);
    public static DrawCommunityCard OnDrawCommunityCard;
    //机会卡
    public delegate void DrawChanceCard(Player player, int quantity);
    public static DrawChanceCard OnDrawChanceCard;
    //关于房子
    public int NumberOfHouses => numberOfHouses;
    void VisualizeHouses()
    {
        switch (numberOfHouses)
        {
            case 0:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 1:
                houses[0].SetActive(true);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 2:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 3:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 4:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
                hotel.SetActive(false);
                break;
            case 5:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(true);
                break;
        }
    }
    public void BuildHousesOrHotel()
    {
        if (monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses++;
            VisualizeHouses();
        }
    }
    public int SellHousesOrHotel()
    {
        if (monopolyNodeType == MonopolyNodeType.Property && numberOfHouses > 0)
        {
            numberOfHouses--;
            VisualizeHouses();
            return houseCost / 2;
        }
        return 0;
    }
    public void ResetNode()
    {
        if (isMortgaged)
        {
            propertyImage.SetActive(true);
            mortgageImage.SetActive(false);
            isMortgaged = false;
        }
        if (monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            VisualizeHouses();
        }
        owner.RemoveProperty(this);
        owner.name = "";
        owner = null;
        OnOwnerUpdate();
    }
    //人类面板
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool haseChanceJailFreeCard, bool haseCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;
    //购买房子面板
    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player currentPlayer);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;
    //购买地铁面板
    public delegate void ShowRailroadBuyPanel(MonopolyNode node, Player currentPlayer);
    public static ShowRailroadBuyPanel OnShowRailroadBuyPanel;
    //购买公共公司面板
    public delegate void ShowUtilityBuyPanel(MonopolyNode node, Player currentPlayer);
    public static ShowUtilityBuyPanel OnShowUtilityBuyPanel;
    private void OnValidate()
    {
        if(nameText != null)
        {
            nameText.text = name;
        }
        //计算
        if(calculateRentAuto)
        {
            if(monopolyNodeType == MonopolyNodeType.Property)
            {
                if(baseRent > 0)
                {
                    price = 3 * (baseRent * 10);
                    //抵押贷款
                    mortgageValue = price / 2;
                    rentwithHouses.Clear();
                    rentwithHouses.Add(baseRent * 5);
                    rentwithHouses.Add(baseRent * 5 * 3);
                    rentwithHouses.Add(baseRent * 5 * 9);
                    rentwithHouses.Add(baseRent * 5 * 16);
                    rentwithHouses.Add(baseRent * 5 * 25);
                }
                else if(baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentwithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)
            {
                mortgageValue = price / 2;
            }
            if (monopolyNodeType == MonopolyNodeType.Railroad)
            {
                mortgageValue = price / 2;
            }
        }

        if (priceText != null)
        {
            priceText.text = price + "$";
        }
        //更新所有者
        OnOwnerUpdate();
        UnMortgageProperty();
    }
    public void UpdateColoerField(Color color)
    {
        if(propertyColoerField != null)
        {
            propertyColoerField.color = color;
        }
    }
    //关于贷款
    public int MortgageProperty()
    {
        isMortgaged = true;
        if(mortgageImage != null)
        {
            mortgageImage.SetActive(true);
        }
        if(propertyImage != null)
        {
            propertyImage.SetActive(false);
        }
        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(false);
        }
        if (propertyImage != null)
        {
            propertyImage.SetActive(true);
        }
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;
    //更新所有者
    public void OnOwnerUpdate()
    {
        if(ownerBar != null)
        {
            if (owner != null && owner.name != "")
            {
                ownerBar.SetActive(true);
                ownerText.text = Owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }

    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;

        //检查类型
        switch(monopolyNodeType)
        {
            case MonopolyNodeType.Null:
                if(!playerIsHuman)//AI
                {

                }
                else
                {

                }
                break;
            case MonopolyNodeType.Property:
                if (!playerIsHuman)//AI
                {
                    //如果房产拥有主人，并且主人不是你，并且房产没有抵押
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //支付房租给的人

                        //计算房租
                        int rentToPay = CalculatePropertyRent();
                        //支付房租
                        currentPlayer.PayRent(rentToPay, owner);
                        //信息提示
                        OnUpdateMassage.Invoke(currentPlayer.name + "给" + Owner.name + "提交租金:" + rentToPay + "$",currentPlayer);

                    }
                    else if(owner == null && currentPlayer.CnAffordNode(price)) //没有主人,并且有足够的钱
                    {
                        //购买
                        OnUpdateMassage.Invoke(currentPlayer.name + "<br>购买了" + this.name,currentPlayer);
                        currentPlayer.BuyProperty(this);
                        //信息提示
                    }
                    else
                    {
                        //买不起或我们是主人
                    }
                }
                else
                {
                    //如果房产拥有主人，并且主人不是你，并且房产没有抵押
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //支付房租给的人

                        //计算房租
                        int rentToPay = CalculatePropertyRent();
                        //支付房租
                        currentPlayer.PayRent(rentToPay, owner);
                        //信息提示
                        OnUpdateMassage.Invoke(currentPlayer.name + "给" + Owner.name + "提交租金:" + rentToPay + "$",currentPlayer);
                    }
                    else if (owner == null) //没有主人
                    {
                        //购买界面
                        OnShowPropertyBuyPanel.Invoke(this,currentPlayer);
                    }
                    else
                    {
                        //买不起或我们是主人
                        //OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                }
                break;
            case MonopolyNodeType.Railroad:
                if (!playerIsHuman)//AI
                {
                    //如果房产拥有主人，并且主人不是你，并且房产没有抵押
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //支付房租给的人

                        //计算房租
                        int rentToPay = CalculateRailroadRent();
                        //支付房租
                        currentPlayer.PayRent(rentToPay, owner);
                        //信息提示
                        OnUpdateMassage.Invoke(currentPlayer.name + "给" + Owner.name + "提交租金:" + rentToPay + "$",currentPlayer);
                    }
                    else if (owner == null && currentPlayer.CnAffordNode(price)) //没有主人,并且有足够的钱
                    {
                        //购买
                        OnUpdateMassage.Invoke(currentPlayer.name + "<br>购买了" + this.name,currentPlayer);
                        currentPlayer.BuyProperty(this);
                        //信息提示
                    }
                    else
                    {
                        //买不起或我们是主人
                    }
                }
                else
                {
                    //如果房产拥有主人，并且主人不是你，并且房产没有抵押
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //支付房租给的人

                        //计算房租
                        int rentToPay = CalculateRailroadRent();
                        //支付房租
                        currentPlayer.PayRent(rentToPay, owner);
                        //信息提示
                        OnUpdateMassage.Invoke(currentPlayer.name + "给" + Owner.name + "提交租金:" + rentToPay + "$",currentPlayer);
                    }
                    else if (owner == null) //没有主人
                    {
                        //购买界面
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //买不起或我们是主人
                    }
                }
                break;
            case MonopolyNodeType.Utility:
                if (!playerIsHuman)//AI
                {
                    //如果房产拥有主人，并且主人不是你，并且房产没有抵押
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //支付房租给的人

                        //计算房租
                        int rentToPay = CalculateUtilityRent();
                        //支付房租
                        currentPlayer.PayRent(rentToPay, owner);
                        //信息提示
                        OnUpdateMassage.Invoke(currentPlayer.name + "给" + Owner.name + "提交租金:" + rentToPay + "$",currentPlayer);
                    }
                    else if (owner == null && currentPlayer.CnAffordNode(price)) //没有主人,并且有足够的钱
                    {
                        //购买
                        OnUpdateMassage.Invoke(currentPlayer.name + "<br>购买了" + this.name,currentPlayer);
                        currentPlayer.BuyProperty(this);
                        //信息提示
                    }
                    else
                    {
                        //买不起或我们是主人
                    }
                }
                else
                {
                    //如果房产拥有主人，并且主人不是你，并且房产没有抵押
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //支付房租给的人

                        //计算房租
                        int rentToPay = CalculateUtilityRent();
                        //支付房租
                        currentPlayer.PayRent(rentToPay, owner);
                        //信息提示
                        OnUpdateMassage.Invoke(currentPlayer.name + "给" + Owner.name + "提交租金:" + rentToPay + "$",currentPlayer);
                    }
                    else if (owner == null) //没有主人
                    {
                        //购买界面
                        OnShowUtilityBuyPanel.Invoke(this,currentPlayer);
                    }
                    else
                    {
                        //买不起或我们是主人
                    }
                }
                break;
            case MonopolyNodeType.Chance:
                OnDrawChanceCard.Invoke(currentPlayer, 1);
                continueTurn = false;
                break;
            case MonopolyNodeType.CommuntiyChest:
                OnDrawCommunityCard.Invoke(currentPlayer,1);
                continueTurn = false;
                break;
            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                OnUpdateMassage.Invoke(currentPlayer.name + "<br>扣除了个人所得税" + price + "$",currentPlayer);
                break;
            case MonopolyNodeType.GoToJail:
                currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(this));
                OnUpdateMassage.Invoke(currentPlayer.name + "<br><color=red>进监狱</color>",currentPlayer);
                continueTurn = false;           
                break;
            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                OnUpdateMassage.Invoke(currentPlayer.name + "<br>获得了税池所有的钱:" + tax + "$",currentPlayer);
                break;
        }
        if (!continueTurn) //等待操作完成
        {
            return;
        }

        if(!playerIsHuman)
        {
            currentPlayer.ChangeState(Player.AiStates.TradingState);
        }
        else
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, currentPlayer.HaseChanceJailFreeCard, currentPlayer.HaseCommunityJailFreeCard);
        }
    }

    //void ContinueGame()
    //{
    //    if (GameManager.instance.RolledADouble)
    //    {
    //        //再来一次
    //        GameManager.instance.RollDice();
    //    }
    //    else
    //    {
    //        //切换玩家
    //        GameManager.instance.SwitchPlayer();
    //    }
    //}

    int CalculatePropertyRent()
    {
        switch(numberOfHouses)
        {
            case 0:
                var(list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
                if (allSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }
                break;
            case 1:
                currentRent = rentwithHouses[0];
                break;
            case 2:
                currentRent = rentwithHouses[1];
                break;
            case 3:
                currentRent = rentwithHouses[2];
                break;
            case 4:
                currentRent = rentwithHouses[3];
                break;
            case 5:
                currentRent = rentwithHouses[4];
                break;
        }

        return currentRent;
    }

    int CalculateUtilityRent()
    {
        List<int> lastRolledDice = GameManager.instance.LastRolledDice;

        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        if (allSame)
        {
            currentRent = (lastRolledDice[0] + lastRolledDice[1]) * 8;
        }
        else
        {
            currentRent = (lastRolledDice[0] + lastRolledDice[1]) * 2;
        }

        return currentRent;
    }

    int CalculateRailroadRent()
    {
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        int amount = 0;
        foreach (var item in list)
        {
            amount += (item.owner == this.owner) ? 1 : 0;
        }
        currentRent = baseRent * (int)Mathf.Pow(2, amount - 1);

        return currentRent;
    }

    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddProperty(this);
        SetOwner(newOwner);
    }
}
