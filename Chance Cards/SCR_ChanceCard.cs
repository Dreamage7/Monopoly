using UnityEngine;

[CreateAssetMenu(fileName = "new Chance Card", menuName = "Monoply/Card/ChanceCard")]
public class SCR_ChanceCard : ScriptableObject
{
    [TextArea]
    public string textOnCard;//描述
    public int rewarMoney;//奖励+
    public int penalityMoney;//惩罚-
    [Header("关于移动到目标的下标")]
    public int moveToBoardIndex = -1;
    [Header("移动地点")]
    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;
    [Header("关于群体")]
    public bool payToPlayer;
    [Header("关于监狱")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("关于街道维修")]
    public bool streetRepairs;
    public int streetRepairsHouse;
    public int streetRepairsHotel;
}
