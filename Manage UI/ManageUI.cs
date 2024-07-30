using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;
    [SerializeField] GameObject managePanel;
    [SerializeField] Transform propertyGrid;
    [SerializeField] GameObject propertySetPrefab;
    Player playerRefernce;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text myMoneyText;
    [SerializeField] TMP_Text systemMessageText;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //managePanel.SetActive(false);
    }
    public void OpenManager()
    {
        playerRefernce = GameManager.instance.GetCurrentPlayer;
        CreateProperty();
        managePanel.SetActive(true);
        UpdateMoneyText();
    }
    public void CloseManager()
    {
        managePanel.SetActive(false);
        CloseProperty();
        UpdateSystemMessage("当前没有消息！");
    }
    void CloseProperty()
    {
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }
    void CreateProperty()
    {
        List<List<MonopolyNode>> processedSet = new List<List<MonopolyNode>>();
        bool notLL = false;
        foreach (var node in playerRefernce.GetMyMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);
            if (processedSet != null)
            {
                notLL = processedSet.Any(n => n == list);
            }
            if (nodeSet != null && !notLL) 
            {
                processedSet.Add(list);
                nodeSet.RemoveAll(n => n.Owner != playerRefernce);
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyGrid, false);
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSet, playerRefernce);
                propertyPrefabs.Add(newPropertySet);
            }
        }
    }
    public void UpdateMoneyText()
    {
        string showMoney = (playerRefernce.ReadMoney >= 0) ? "<color=green>" + playerRefernce.ReadMoney : "<color=red>" + playerRefernce.ReadMoney;
        myMoneyText.text = "资产：" + showMoney + "$";
    }
    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }
    public void AutoHandleFunds()
    {
        if (playerRefernce.ReadMoney > 0)
        {
            UpdateSystemMessage("你当前不需要处理债务问题！");
            return;
        }
        playerRefernce.HandleInsufficientFunds(0);
        CloseProperty();
        CreateProperty();
        UpdateMoneyText();
    }
}
