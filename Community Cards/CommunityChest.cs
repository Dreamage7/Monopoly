using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommunityChest : MonoBehaviour
{
    public static CommunityChest instance;
    [SerializeField] List<SCR_CommunityCard> cards = new List<SCR_CommunityCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHandlerBackground;
    [SerializeField] float showTime = 3f;
    [SerializeField] Button closeCardButton;

    List<SCR_CommunityCard> cardPoolDraw = new List<SCR_CommunityCard>();//抽牌堆
    List<SCR_CommunityCard> cardPoolFold = new List<SCR_CommunityCard>();//弃牌堆

    SCR_CommunityCard jailFreeCard;
    SCR_CommunityCard pickedCard;
    Player currentPlayer;
    //人类面板
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool haseChanceJailFreeCard, bool haseCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;
    private void OnEnable()
    {
        MonopolyNode.OnDrawCommunityCard += DrawCard;
    }

    private void OnDisable()
    {
        MonopolyNode.OnDrawCommunityCard -= DrawCard;
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
            SCR_CommunityCard tmpeCard = cardPoolDraw[index];
            cardPoolDraw[index] = cardPoolDraw[i];
            cardPoolDraw[i] = tmpeCard;
        }
    }

    void DrawCard(Player player,int quantity)//抽牌
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
            Invoke("ApplyCardEffect",showTime);
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }

    public void ApplyCardEffect()//放在关闭按钮上的方法
    {
        bool isMoving = false;
        if (pickedCard.rewarMoney != 0 && !pickedCard.collectFromPlayer)
        {
            currentPlayer.CollectMoney(pickedCard.rewarMoney);
        }
        else if(pickedCard.penalityMoney != 0)
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
            MonopolyBoard.instance.MovePlayerToken(stepsToMove,currentPlayer);
        }
        else if (pickedCard.collectFromPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayerList;

            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    int amount = Mathf.Min(player.ReadMoney,pickedCard.rewarMoney);
                    player.PayMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);
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
        else if(pickedCard.jailFreeCard)
        {
            currentPlayer.AddCommunityJailFreeCard();
        }
        cardHandlerBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if(!isMoving)
            {
                GameManager.instance.Continue();
            }
        }
        else
        {
            if (!isMoving)
            {
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble, currentPlayer.HaseChanceJailFreeCard, currentPlayer.HaseCommunityJailFreeCard);
            }
        }
    }
    public void AddBackJailFreeCard()
    {
        cardPoolFold.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
