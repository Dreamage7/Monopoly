using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] List<Player> playerListBak = new List<Player>();
    [SerializeField] int currentPlayer; //当前玩家索引
    [Header("全局游戏设置")] 
    [SerializeField] int maxTurnsInJail = 2; //在监狱里待的时间
    [SerializeField] int startMoney = 600; //初始资金
    [SerializeField] int goMoney = 100;
    [SerializeField] float secondsBetweenTurns = 3;
    [Header("玩家面板信息")]
    [SerializeField] GameObject playerInfoPrefab; //玩家面板的预制件
    [SerializeField] Transform playerPanel; //玩家面板位置
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();
    [Header("游戏结束信息")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text winnerNameText;
    [SerializeField] GameObject overPanel;
    [Header("所有面板")]
    [SerializeField] GameObject[] Panels;
    [Header("物理骰子")]
    [SerializeField] Dice dice1;
    [SerializeField] Dice dice2;
    //关于骰子
    List<int> rolledDice = new List<int>();
    bool rolledADouble;
    int doubleRollCount;
    bool hasRolledDice;
    //[SerializeField] bool Debugbug;
    //[SerializeField] int bugde_1;
    //[SerializeField] int bugde_2;
    //public bool doubleRollDiceTwo;
    public List<int> LastRolledDice => rolledDice;
    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;
    public bool HasRolledDice => hasRolledDice;
    //税池
    int taxPool = 0;
    public void AddTaxToPool(int amount)
    {
        taxPool += amount;
    }
    public int GetTaxPool()
    {
        int currentTaxCollected = taxPool;
        taxPool = 0;
        return currentTaxCollected;
    }
    //关于GO
    public int GetGoMoney => goMoney;
    //关于等待时间或是其他什么
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayerList => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];
    //信息
    public delegate void UpdateMassage(string _maggage,Player currentPlayer);
    public static UpdateMassage OnUpdateMassage;
    //人类面板
    public delegate void ShowHumanPanel(bool activatePanel,bool activateRollDice,bool activateEndTurnbool,bool haseChanceJailFreeCard, bool haseCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    private void Start()
    {
        startMoney = GameSetting.startGameMoney;
        foreach (var panel in Panels)
        {
            panel.SetActive(false);
        }
        CameraSwitcher.instance.SwitchToDice();
        gameOverPanel.SetActive(false);
        overPanel.SetActive(false);
        Inititialize();
        Invoke("startTop",0.8f); 
        StartCoroutine(StartGame());
        OnUpdateMassage.Invoke("游戏即将开始",GetCurrentPlayer);
    }
    public void startTop()
    {
        CameraSwitcher.instance.SwitchToTopDown();
        currentPlayer = Random.Range(0, playerList.Count);
        playerList[currentPlayer].ActivateSelector(true);
    }
    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3f);
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            RollPhysicalDice();
            OnShowHumanPanel.Invoke(false, false, false, false, false);
        }
        else
        {
            OnShowHumanPanel.Invoke(true, true, false, false, false);
        }
    }

    void Inititialize()
    {
        if (GameSetting.settingsList.Count == 0)
        {
            //Debug.Log("错误");
            return;
        }
        foreach (var setting in GameSetting.settingsList)
        {
            Player player = new Player();
            player.name = setting.playerName;
            player.playerType = (Player.PlayerType)setting.playerType;
            playerList.Add(player);

            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            //生成代币
            GameObject newToken = Instantiate(playerTokenList[setting.colorType], SetTokenV3(gameBoard.route[0].transform.position), Quaternion.identity);
            Color tokencolor = newToken.GetComponentInChildren<MeshRenderer>().material.color;
            player.Inititialize(gameBoard.route[0], startMoney, info, newToken, tokencolor);
        }
        //for (int i = 0; i < playerList.Count; i++)
        //{
        //    GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
        //    PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
        //    //随机玩家代币索引
        //    int randomIndex = Random.Range(0,playerTokenList.Count);
        //    //生成代币
        //    GameObject newToken = Instantiate(playerTokenList[randomIndex], SetTokenV3(gameBoard.route[0].transform.position),Quaternion.identity);
        //    Color tokencolor = newToken.GetComponentInChildren<MeshRenderer>().material.color;
        //    playerList[i].Inititialize(gameBoard.route[0], startMoney, info, newToken,tokencolor);
        //}
        //playerList[currentPlayer].ActivateSelector(true);
    }

    public Vector3 SetTokenV3(Vector3 oldV3) //,int playerindex,int playerCount
    {
        /*if(playerCount == 4)
        {
            switch (playerindex)
            {
            case 0:
                Vector3 newV3_0 = new Vector3(oldV3.x - 0.8f, oldV3.y + 1.01f, oldV3.z - 0.8f);
                return newV3_0;
            case 1:
                Vector3 newV3_1 = new Vector3(oldV3.x + 0.8f, oldV3.y + 1.01f, oldV3.z - 0.8f);
                return newV3_1;
            case 2:
                Vector3 newV3_2 = new Vector3(oldV3.x - 0.8f, oldV3.y + 1.01f, oldV3.z + 0.8f);
                return newV3_2;
            case 3:
                Vector3 newV3_3 = new Vector3(oldV3.x + 0.8f, oldV3.y + 1.01f, oldV3.z - 0.8f);
                return newV3_3;
            }
        }
        else if (playerCount == 3)
        {
            switch (playerindex)
            {
                case 0:
                    Vector3 newV3_0 = new Vector3(oldV3.x - 0.8f, oldV3.y + 1.01f, oldV3.z - 0.8f);
                    return newV3_0;
                case 1:
                    Vector3 newV3_1 = new Vector3(oldV3.x + 0.8f, oldV3.y + 1.01f, oldV3.z - 0.8f);
                    return newV3_1;
                case 2:
                    Vector3 newV3_2 = new Vector3(oldV3.x, oldV3.y + 1.01f, oldV3.z + 0.8f);
                    return newV3_2;
            }
        }
        else if (playerCount == 2)
        {
            switch (playerindex)
            {
                case 0:
                    Vector3 newV3_0 = new Vector3(oldV3.x - 0.8f, oldV3.y + 1.01f, oldV3.z);
                    return newV3_0;
                case 1:
                    Vector3 newV3_1 = new Vector3(oldV3.x + 0.8f, oldV3.y + 1.01f, oldV3.z);
                    return newV3_1;
            }
        }*/
        Vector3 newV3 = new Vector3(oldV3.x, oldV3.y + 1.01f, oldV3.z);
        return newV3;
    }

    public void RollPhysicalDice()
    {
        CheckForJailFree();
        rolledDice.Clear();
        dice1.RollDice();
        dice2.RollDice();
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            OnShowHumanPanel.Invoke(true, false, false, playerList[currentPlayer].HaseChanceJailFreeCard, playerList[currentPlayer].HaseCommunityJailFreeCard);
        }
        CameraSwitcher.instance.SwitchToDice();
    }

    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count >= 2)
        {
            RollDice();
        }
    }

    void CheckForJailFree()
    {
        //免狱卡
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HaseChanceJailFreeCard)
            {
                playerList[currentPlayer].UseChanceJailFreeCard();
            }
            else if (playerList[currentPlayer].HaseCommunityJailFreeCard)
            {
                playerList[currentPlayer].UseCommunityJailFreeCard();
            }
        }
    }

    void RollDice() //仍骰子方法
    {
        bool allowedToMove = true;
        hasRolledDice = true;
        ////定义骰子数量
        //rolledDice = new int[2];
        ////仍骰子并存储它们
        //rolledDice[0] = Random.Range(1, 7);
        //rolledDice[1] = Random.Range(1, 7);
        //只会扔双倍
        //if (doubleRollDiceTwo)
        //{
        //    rolledDice[0] = rolledDice[1];
        //}
        //if (Debugbug)
        //{
        //    rolledDice[0] = bugde_1;
        //    rolledDice[1] = bugde_2;
        //}
        //Debug.Log("点数1:" + rolledDice[0] + ";点数2:" + rolledDice[1]);
        //检查骰子的双倍情况
        rolledADouble = rolledDice[0] == rolledDice[1];
        //骰子的双倍几次了，3次 => 进监狱 => 结束行动
        //是否有人在监狱
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();
            if(rolledADouble)
            {       
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMassage.Invoke(playerList[currentPlayer].name + "<br>扔出了双倍提前离开了监狱",GetCurrentPlayer);
                doubleRollCount++;
                if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
                {
                    Invoke("RollPhysicalDice", secondsBetweenTurns);
                }          
                return;
            }
            else if(playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMassage.Invoke(playerList[currentPlayer].name + "<br>刑满释放,龙王归来",GetCurrentPlayer);
                if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
                {
                    Invoke("RollPhysicalDice", secondsBetweenTurns);
                }
                return;
            }
            else
            {
                allowedToMove = false;
            }
        }
        else
        {
            //重置
            if(!rolledADouble)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;
                if (doubleRollCount >= 3)
                {
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMassage.Invoke(playerList[currentPlayer].name + "<br>连续3次双倍需要<color=red>进监狱!</color>",GetCurrentPlayer);
                    if (GetCurrentPlayer.playerType == Player.PlayerType.HUMAN)
                    {
                        OnShowHumanPanel.Invoke(true, false, false, playerList[currentPlayer].HaseChanceJailFreeCard, playerList[currentPlayer].HaseCommunityJailFreeCard);
                    }
                    return;
                }
            }
        }
        //是否可以离开监狱

        //移动
        if (allowedToMove)
        {
            OnUpdateMassage.Invoke(playerList[currentPlayer].name + "<br>开始移动" + rolledDice[0] + "&" + rolledDice[1] + "步",GetCurrentPlayer);
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            //Debug.Log("切换玩家");
            OnUpdateMassage.Invoke(playerList[currentPlayer].name + "<br>龙场悟道中.......",GetCurrentPlayer);
            StartCoroutine(DelayBetweenSwitchPlayer());
        }
        //用户界面
        //if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        //{
        //    OnShowHumanPanel.Invoke(true, false, false, playerList[currentPlayer].HaseChanceJailFreeCard, playerList[currentPlayer].HaseCommunityJailFreeCard);
        //}
    }

    IEnumerator DelayBeforeMove(int rolledDice) //行动前拖延
    {
        CameraSwitcher.instance.SwitchToPlayer(GetCurrentPlayer.MyToken.transform);
        yield return new WaitForSeconds(secondsBetweenTurns); //等待几秒继续执行
        //是否允许移动
        if (playerList[currentPlayer] != null)
        {
            gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        }
        //反之改变位置
    }

    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns); //等待几秒继续执行
        SwitchPlayer();
    }

    public void SwitchPlayer() //切换玩家方法
    {
        CameraSwitcher.instance.SwitchToTopDown();
        currentPlayer++;
        doubleRollCount = 0;
        hasRolledDice = false;
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }
        DeactiaveArrows();
        playerList[currentPlayer].ActivateSelector(true);
        //是否在监狱

        //是AI
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            RollPhysicalDice();
            OnShowHumanPanel.Invoke(false, false, false, false, false);
        }
        else
        {
            OnShowHumanPanel.Invoke(true,true,false, playerList[currentPlayer].HaseChanceJailFreeCard, playerList[currentPlayer].HaseCommunityJailFreeCard);
        }
        //是玩家
    }

    //-------------------------------------UI------------------------------------
    void DeactiaveArrows()
    {
        foreach (var player in playerList)
        {
            player.ActivateSelector(false);
        }
    }
    //-------------------------------AI游戏状态------------------------------------
    public void Continue()
    {
        if (playerList.Count > 1)
        {
            Invoke("ContinueGame", SecondsBetweenTurns);
        }
    }
    void ContinueGame()
    {
        if (RolledADouble)
        {
            //再来一次
            //RollDice();
            RollPhysicalDice();
        }
        else
        {
            //切换玩家
            SwitchPlayer();   
        }
    }
    //-------------------------------游戏结束------------------------------------
    public void RemovePlayer(Player owner)
    {
        Destroy(owner.MyToken);
        playerListBak.Add(owner);
        playerList.Remove(owner);     
        //是否只剩一名玩家游戏结束
        CheckForGameOver();
    }
    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {
            OnUpdateMassage.Invoke(playerList[0].name + "获胜！<br>游戏结束!", playerList[0]);
            gameOverPanel.SetActive(true);
            winnerNameText.text = playerList[0].name;
        }
    }
    public void HumanBankrupt()
    {
        playerList[currentPlayer].Bankrupt();
    }

    public void StartGameCopy()
    {
        if (playerListBak.Count != 0)
        {
            playerList.AddRange(playerListBak);
            playerListBak.Clear();
        }
        for (int i = playerList.Count-1; i >=0 ; i--)
        {
            playerList[i].Bankrupt(1);
            Destroy(playerList[i].MyInfo.gameObject);
            Destroy((playerList[i].MyToken != null)? playerList[i].MyToken:null);
            playerList.Remove(playerList[i]);
        }
        Start();
    }
    public void ExitMainMenu()
    {
        GameSetting.ClearSetting();
        SceneManager.LoadScene("MainMenu");
    }
    //-------------------------------免狱卡使用------------------------------------
    public void UseJailOne()
    {
        playerList[currentPlayer].UseChanceJailFreeCard();
    }
    public void UseJailTwo()
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }
}
