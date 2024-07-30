using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChanceField : MonoBehaviour
{
    public static ChanceField instance;
    [SerializeField] List<SCR_ChanceCard> cards = new List<SCR_ChanceCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHandlerBackground;
    [SerializeField] float showTime = 3f;
    [SerializeField] Button closeCardButton;

    List<SCR_ChanceCard> cardPoolDraw = new List<SCR_ChanceCard>();//抽牌堆
    List<SCR_ChanceCard> cardPoolFold = new List<SCR_ChanceCard>();//弃牌堆

    SCR_ChanceCard jailFreeCard;
    SCR_ChanceCard pickedCard;
    Player currentPlayer;
    //人类面板
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool haseChanceJailFreeCard, bool haseCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;
    private void OnEnable()
    {
        MonopolyNode.OnDrawChanceCard += DrawCard;
    }

    private void OnDisable()
    {
        MonopolyNode.OnDrawChanceCard -= DrawCard;
    }
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //cardHandlerBackground.SetActive(false);
        cardPoolDraw.AddRange(cards);
        ShuffleCards();
    }

    void ShuffleCards() //洗牌
    {
        for (int i = 0; i < cardPoolDraw.Count; i++)
        {
            int index = Random.Range(0, cardPoolDraw.Count);
            SCR_ChanceCard tmpeCard = cardPoolDraw[index];
            cardPoolDraw[index] = cardPoolDraw[i];
            cardPoolDraw[i] = tmpeCard;
        }
    }

    void DrawCard(Player player, int quantity)//抽牌
    {
        pickedCard = cardPoolDraw[0];
        cardPoolDraw.RemoveAt(0);

        if (pickedCard.jailFreeCard)
        {
            jailFreeCard = pickedCard;
        }
        else
        {
            cardPoolFold.Add(pickedCard);
        }
        
        if (cardPoolDraw.Count == 0)
        {
            cardPoolDraw.AddRange(cardPoolFold);
            cardPoolFold.Clear();
            ShuffleCards();
        }

        currentPlayer = player;
        cardText.text = pickedCard.textOnCard;
        cardHandlerBackground.SetActive(true);

        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }

    public void ApplyCardEffect()//放在关闭按钮上的方法
    {
        bool isMoving = false;
        if (pickedCard.rewarMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewarMoney);
        }
        else if (pickedCard.penalityMoney != 0 && !pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney);
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;

            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;
            int stepsToMove = 0;
            if (currentIndex < pickedCard.moveToBoardIndex)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }
            MonopolyBoard.instance.MovePlayerToken(stepsToMove, currentPlayer);
        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayerList;

            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    int amount = Mathf.Min(currentPlayer.ReadMoney, pickedCard.penalityMoney);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.PayMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
            int totalCosts = allBuildings[0] * pickedCard.streetRepairsHouse + allBuildings[1] * pickedCard.streetRepairsHotel;
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = true;

            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if (pickedCard.jailFreeCard)
        {
            currentPlayer.AddChanceJailFreeCard();
        }
        else if (pickedCard.moveStepsBackwards != 0)
        {
            isMoving = true;

            int steps = pickedCard.moveStepsBackwards;
            MonopolyBoard.instance.MovePlayerToken(steps,currentPlayer);
        }
        else if (pickedCard.nextRailroad)
        {
            isMoving = true;

            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Railroad,currentPlayer);
        }
        else if (pickedCard.nextUtility)
        {
            isMoving = true;

            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Utility, currentPlayer);
        }
        cardHandlerBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if (!isMoving)
            {
                GameManager.instance.Continue();
            }
        }
        else
        {
            if (!isMoving)
            {
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble,currentPlayer.HaseChanceJailFreeCard,currentPlayer.HaseCommunityJailFreeCard);
            }
        }
    }
    public void AddBackJailFreeCard()
    {
        cardPoolFold.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
