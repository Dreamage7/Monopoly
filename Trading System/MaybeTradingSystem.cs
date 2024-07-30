using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class MaybeTradingSystem : MonoBehaviour
{
    public static MaybeTradingSystem instance;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultMessagePanel;
    [SerializeField] TMP_Text resultMessageText;
    [Header("交易人左侧")]
    [SerializeField] Transform cardGrid_L;
    [SerializeField] GameObject cardPrefab_L;
    [SerializeField] ToggleGroup toggleGroup_L;
    [SerializeField] TMP_Text yourMoneyText_L;
    [SerializeField] TMP_Text offerMoney_L;
    [SerializeField] Slider moneySilder_L;
    [SerializeField] TMP_InputField inputField_L;
    List<GameObject> cardPrefabList_L = new List<GameObject>();
    Player playerRefere_L;
    [Header("中间区域")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject playerPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();
    [SerializeField] Button makeOfferButton;
    [SerializeField] Button closeButton;
    [Header("被交易人右侧")]
    [SerializeField] TMP_Text playerNameText_R;
    [SerializeField] Transform cardGrid_R;
    [SerializeField] GameObject cardPrefab_R;
    [SerializeField] ToggleGroup toggleGroup_R;
    [SerializeField] TMP_Text yourMoneyText_R;
    [SerializeField] TMP_Text offerMoney_R;
    [SerializeField] Slider moneySilder_R;
    [SerializeField] TMP_InputField inputField_R;
    List<GameObject> cardPrefabList_R = new List<GameObject>();
    Player playerRefere_R;
    [Header("AI交易面板")]
    [SerializeField] GameObject tradeHumanPanel;
    [SerializeField] TMP_Text leftMessageText, rightMessageText, leftMoneyText, rightMoneyText;
    [SerializeField] GameObject leftCard, rightCard;
    //
    Player currentPlayerCopy, nodeOwnerCopy;
    int requestedMoneyCopy, offeredMoneyCopy;
    MonopolyNode requestedNodeCopy, offeredNodeCopy;
    //信息
    public delegate void UpdateMassage(string _maggage, Player currentPlayer);
    public static UpdateMassage OnUpdateMassage;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //tradePanel.SetActive(false);
        //resultMessagePanel.SetActive(false);
        //tradeHumanPanel.SetActive(false);
    }
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null;
        MonopolyNode requestedNode = null;
        foreach (var node in currentPlayer.GetMyMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);
            bool notAllPurchased = list.Any(n => n.Owner == null);
            if (allSame || processedSet == list || notAllPurchased)
            {
                processedSet = list;
                continue;
            }
            if (list.Count == 2)
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                if (requestedNode != null)
                {
                    MakeTradeDecision(currentPlayer,requestedNode.Owner,requestedNode);
                    break;
                }
            }
            if (list.Count >= 3)
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
        }
        if (requestedNode == null)
        {
            currentPlayer.ChangeState(Player.AiStates.Idle);
        }
    }
    void MakeTradeDecision(Player currentPlayer,Player nodeOwner,MonopolyNode requestedNode)
    {
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))
        {
            MakeTradeOffer(currentPlayer,nodeOwner,requestedNode,null,CalculateValueOfNode(requestedNode),0);
            return;
        }
        bool foundDecision = false;
        foreach (var node in currentPlayer.GetMyMonopolyNodes)
        {
            var checkedSet = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node).list;
            if (checkedSet.Contains(requestedNode))
            {
                continue;
            }
            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)
            {
                if (CalculateValueOfNode(node) + currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))
                {
                    int diffrence = CalculateValueOfNode(requestedNode) - CalculateValueOfNode(node);
                    if (diffrence > 0)
                    {
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, diffrence, 0);
                    }
                    else
                    {
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0,Mathf.Abs(diffrence));
                    }
                    foundDecision = true;
                    break;
                }
            }
        }
        if (!foundDecision)
        {
            currentPlayer.ChangeState(Player.AiStates.Idle);
        }
    }
    void MakeTradeOffer(Player currentPlayer,Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode,int offeredMoney,int requestedMoney)
    {
        if (nodeOwner.playerType == Player.PlayerType.AI)
        {
            ConsiderTradeOffer(currentPlayer,nodeOwner,requestedNode,offeredNode,offeredMoney,requestedMoney);
        }
        else
        {
            //UI
            ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = CalculateValueOfNode(requestedNode) - CalculateValueOfNode(offeredNode) - offeredMoney + requestedMoney;
        if (requestedNode == null && offeredNode != null && (requestedMoney < nodeOwner.ReadMoney/4 || (requestedMoney - 10) <= CalculateValueOfNode(offeredNode)) && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
            return;
        }
        if (valueOfTheTrade <= 0 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            Trade(currentPlayer,nodeOwner,requestedNode,offeredNode,offeredMoney,requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
        }
        else
        {
            OnUpdateMassage.Invoke(nodeOwner.name + "拒绝了" + currentPlayer.name + "的交易",currentPlayer);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(false);
            }
            //拒绝交易
        }
    }
    public int CalculateValueOfNode(MonopolyNode requestedNode)
    {
        int Value = 0;
        if (requestedNode != null)
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)
            {
                Value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;
            }
            else
            {
                Value = requestedNode.price;
            }
            return Value;
        }       
        return Value;
    }
    //交易完成
    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (requestedNode != null)
        {
            currentPlayer.PayMoney(offeredMoney);
            requestedNode.ChangeOwner(currentPlayer);
            nodeOwner.CollectMoney(offeredMoney);
            nodeOwner.PayMoney(requestedMoney);
            currentPlayer.CollectMoney(requestedMoney);
            if (offeredNode != null)
            {
                offeredNode.ChangeOwner(nodeOwner);
            }
            string trophies = (offeredNode != null) ? offeredNode.name + "和" + offeredMoney + "$" : offeredMoney + "$";
            OnUpdateMassage.Invoke(currentPlayer.name + "和" + nodeOwner.name + "正在进行交易！<br>" + currentPlayer.name + "获得了" + requestedNode.name + ";" + nodeOwner.name + "获得了" + trophies,currentPlayer);
        }
        else if (offeredNode != null && requestedNode == null)
        {
            nodeOwner.PayMoney(requestedMoney);
            currentPlayer.CollectMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);
            OnUpdateMassage.Invoke(currentPlayer.name + "和" + nodeOwner.name + "正在进行交易！<br>" + currentPlayer.name + "获得了" + requestedMoney + "$;" + nodeOwner.name + "获得了" + offeredNode.name,currentPlayer);
        }
        if (currentPlayer.playerType == Player.PlayerType.HUMAN)
        {
            CloseTradePanel();
        }
        else
        {
            currentPlayer.ChangeState(Player.AiStates.Idle);
        }
    }
    public void OpenTradePanel()//打开交易面板按钮
    {
        playerRefere_L = GameManager.instance.GetCurrentPlayer;
        CreateLeftPanel();
        CreateMiddlePanel();
        moneySilder_R.maxValue = 0;
    }
    void CreateLeftPanel()//左边
    {
        List<MonopolyNode> referenceNode = playerRefere_L.GetMyMonopolyNodes;
        for (int i = 0; i < referenceNode.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab_L,cardGrid_L,false);
            MaybeTradingCardUI tradeCardUI = tradeCard.GetComponent<MaybeTradingCardUI>();
            tradeCardUI.SetCard(referenceNode[i],toggleGroup_L);
            cardPrefabList_L.Add(tradeCard);
        }
        yourMoneyText_L.text = "资产：" + playerRefere_L.ReadMoney + "$";
        moneySilder_L.maxValue = playerRefere_L.ReadMoney;
        moneySilder_L.value = 0;
        UpdateLeftSlider(moneySilder_L.value);
        tradePanel.SetActive(true);
    }
    void CreateMiddlePanel()//中间
    {
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();
        List<Player> allPlayers = new List<Player>();
        allPlayers.AddRange(GameManager.instance.GetPlayerList);
        allPlayers.Remove(playerRefere_L);
        foreach (var player in allPlayers)
        {
            GameObject newPlayer = Instantiate(playerPrefab,buttonGrid,false);
            newPlayer.GetComponent<TradePlayerButton>().SetPlayer(player);
            playerButtonList.Add(newPlayer);
        }
    }
    public void ShowRightPlayer(Player player)//右边
    {
        if (!inputField_R.interactable)
        {
            inputField_R.interactable = true;
        }
        playerRefere_R = player;
        ClearRigthtPanel();
        List<MonopolyNode> referenceNode = playerRefere_R.GetMyMonopolyNodes;
        playerNameText_R.text = player.name;
        yourMoneyText_R.text = "资产：" + player.ReadMoney + "$";    
        for (int i = 0; i < referenceNode.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab_R, cardGrid_R, false);
            MaybeTradingCardUI tradeCardUI = tradeCard.GetComponent<MaybeTradingCardUI>();
            tradeCardUI.SetCard(referenceNode[i], toggleGroup_R);
            cardPrefabList_R.Add(tradeCard);
        }
        moneySilder_R.maxValue = player.ReadMoney;
        moneySilder_R.value = 0;
        UpdateRightSlider(moneySilder_R.value);
    }
    void ClearAll()
    {
        playerNameText_R.text = "选择一名交易对象";
        yourMoneyText_R.text = "资产：???";
        moneySilder_R.maxValue = 0;
        UpdateRightSlider(moneySilder_R.value);
        inputField_L.text = null;
        inputField_R.text = null;
        inputField_R.interactable = false;

        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        for (int i = cardPrefabList_L.Count - 1; i >= 0; i--)
        {
            Destroy(cardPrefabList_L[i]);
        }
        cardPrefabList_L.Clear();

        for (int i = cardPrefabList_R.Count - 1; i >= 0; i--)
        {
            Destroy(cardPrefabList_R[i]);
        }
        cardPrefabList_R.Clear();
    }
    void ClearRigthtPanel()
    {
        for (int i = cardPrefabList_R.Count - 1; i >= 0; i--)
        {
            Destroy(cardPrefabList_R[i]);
        }
        cardPrefabList_R.Clear();
        moneySilder_R.maxValue = 0;
        moneySilder_R.value = 0;
        UpdateRightSlider(moneySilder_R.value);
    }
    public void UpdateLeftSlider(float value)
    {
        offerMoney_L.text = "支付金额：" + moneySilder_L.value + "$";
        inputField_L.text = moneySilder_L.value.ToString();
    }
    public void UpdateRightSlider(float value)
    {
        offerMoney_R.text = "返还金额：" + moneySilder_R.value + "$";
        inputField_R.text = moneySilder_L.value.ToString();
    }
    public void UpdateMoneyLeftText()
    {
        bool NotNull = float.TryParse(inputField_L.text,out float moneyText);
        if (NotNull != false)
        {
            moneySilder_L.value = moneyText;
            if (moneyText > moneySilder_L.maxValue)
            {
                inputField_L.text = moneySilder_L.maxValue.ToString();
            }
        }
        else
        {
            moneySilder_L.value = 0;
        }
    }
    public void UpdateMoneyRightText()
    {
        bool NotNull = float.TryParse(inputField_R.text, out float moneyText);
        if (NotNull != false)
        {
            moneySilder_R.value = moneyText;
            if (moneyText > moneySilder_R.maxValue)
            {
                inputField_R.text = moneySilder_R.maxValue.ToString();
            }
        }
        else
        {
            moneySilder_R.value = 0;
        }
    }
    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
        ClearAll();
    }
    //取消
    public void ResetUI()
    {
        moneySilder_L.value = 0;
        Toggle yesToggle = toggleGroup_L.ActiveToggles().FirstOrDefault();
        if (yesToggle != null)
        {
            yesToggle.isOn = false;
        }
        inputField_L.text = null;
        if (playerRefere_R == null)
        {
            return;
        }       
        playerNameText_R.text = "选择一名交易对象";
        yourMoneyText_R.text = "资产：???";
        ClearRigthtPanel();
        inputField_R.text = null;
        inputField_R.interactable = false;
    }
    //报价
    public void MakeOffButton()
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;
        if (playerRefere_R == null)
        {
            return;
        }
        Toggle offeredToggle = toggleGroup_L.ActiveToggles().FirstOrDefault();
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<MaybeTradingCardUI>().NodeRefernce;
        }
        Toggle requestedTogger = toggleGroup_R.ActiveToggles().FirstOrDefault();
        if (requestedTogger != null)
        {
            requestedNode = requestedTogger.GetComponentInParent<MaybeTradingCardUI>().NodeRefernce;
        }
        MakeTradeOffer(playerRefere_L,playerRefere_R,requestedNode,offeredNode, (int)moneySilder_L.value, (int)moneySilder_R.value);
    }
    void TradeResult(bool accepted)
    {
        if (accepted)
        {
            resultMessageText.text = playerRefere_R.name + "<color=green>同意</color>了你的提议!";
        }
        else
        {
            resultMessageText.text = playerRefere_R.name + "<color=red>拒绝</color>了你的提议!";
        }
        resultMessagePanel.SetActive(true);
    }
    void ShowTradeOfferPanel(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        currentPlayerCopy = currentPlayer;
        nodeOwnerCopy = nodeOwner;
        requestedNodeCopy = requestedNode;
        offeredNodeCopy = offeredNode;
        offeredMoneyCopy = offeredMoney;
        requestedMoneyCopy = requestedMoney;
        if (offeredNode != null)
        {
            leftCard.GetComponent<MaybeTradingCardUI>().SetCard(offeredNode);
        }
        if (requestedNode != null)
        {
            rightCard.GetComponent<MaybeTradingCardUI>().SetCard(requestedNode);
        }
        leftMessageText.text = "提议人：" + currentPlayer.name;
        rightMessageText.text = "被交易人：" + nodeOwner.name;
        leftMoneyText.text = "+" + offeredMoney + "$";
        rightMoneyText.text = "-" + requestedMoney + "$";
        tradeHumanPanel.SetActive(true);
    }
    public void AcceptOffer()
    {
        Trade(currentPlayerCopy,nodeOwnerCopy,requestedNodeCopy,offeredNodeCopy,offeredMoneyCopy,requestedMoneyCopy);
        ResetOffer();
        tradeHumanPanel.SetActive(false);
    }
    public void RejectOffer()
    {
        currentPlayerCopy.ChangeState(Player.AiStates.Idle);
        ResetOffer();
        tradeHumanPanel.SetActive(false);
    }
    void ResetOffer()
    {
        leftCard.GetComponent<MaybeTradingCardUI>().ResetCard();
        rightCard.GetComponent<MaybeTradingCardUI>().ResetCard();
        currentPlayerCopy = null;
        nodeOwnerCopy = null;
        requestedNodeCopy = null;
        offeredNodeCopy = null;
        offeredMoneyCopy = 0;
        requestedMoneyCopy = 0;
    }
}
