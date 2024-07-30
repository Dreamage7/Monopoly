using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonopolyBoard : MonoBehaviour
{
    public static MonopolyBoard instance;

    public List<MonopolyNode> route = new List<MonopolyNode>(); //路线

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodeInSetList = new List<MonopolyNode>();
    }

    public List<NodeSet> nodeSetList = new List<NodeSet>();

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

    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentsInChildren<Transform>())
        {
            if(node.GetComponent<MonopolyNode>() != null)
            {
                route.Add(node.GetComponent<MonopolyNode>());
            }
        }
    }

    void OnDrawGizmos()
    {
        if (route.Count>1)
        {
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i + 1 < route.Count) ? route[i+1].transform.position : current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);
            }
        }
    }

    /// <summary>
    /// 移动我的代币
    /// </summary>
    /// <param name="steps">步数</param>
    /// <param name="player">玩家</param>
    public void MovePlayerToken(int steps,Player player)
    {
        StartCoroutine(MovePlayerInSteps(steps,player));
    }

    public void MovePlayerToken(MonopolyNodeType nodeType, Player player)
    {
        int indexOfNextNodeType = -1;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0;

        while (indexOfNextNodeType == -1 && nodeSearches < route.Count)
        {
            if (route[startSearchIndex].monopolyNodeType == nodeType)
            {
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            nodeSearches++;
        }
        if (indexOfNextNodeType == -1)
        {
            Debug.Log("未找到节点");
            return;
        }
        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }

    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        yield return new WaitForSeconds(0.5f);

        int stepsLeft = steps; //步数
        GameObject tokenToMove = player.MyToken; //玩家的游戏代币
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); //当前在地图上的位置下标
        bool moveOverGo = false;
        bool isMovingForward = steps > 0;

        if (isMovingForward)
        {
            while (stepsLeft > 0)
            {
                indexOnBoard++;
                if (indexOnBoard > route.Count - 1)
                {
                    indexOnBoard = 0;
                    moveOverGo = true;
                }
                //开始和结束的位置
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = GameManager.instance.SetTokenV3(route[indexOnBoard].transform.position);
                //执行移动
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null;
                }
                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft < 0)
            {
                indexOnBoard--;
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count-1;
                }
                //开始和结束的位置
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = GameManager.instance.SetTokenV3(route[indexOnBoard].transform.position);
                //执行移动
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null;
                }
                stepsLeft++;
            }
        }      
        //经过初始点获得钱
        if(moveOverGo)
        {
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        //更新新的位置
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    bool MoveToNextNode(GameObject tokenToMove,Vector3 endPos,float speed)
    {
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position,endPos,speed * Time.deltaTime));
    }

    public (List<MonopolyNode> list, bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)
    {
        bool allSame = false;
        foreach (var nodeSet in nodeSetList)
        {
            if (nodeSet.nodeInSetList.Contains(node))
            {
                allSame = nodeSet.nodeInSetList.All(_node => _node.Owner == node.Owner);
                return (nodeSet.nodeInSetList, allSame);
            }
        }
        return (null,allSame);
    }
}