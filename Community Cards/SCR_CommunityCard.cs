using UnityEngine;

[CreateAssetMenu(fileName = "new Community Card",menuName = "Monoply/Card/CommunityCard")]
public class SCR_CommunityCard : ScriptableObject
{
    [TextArea]
    public string textOnCard;//描述
    public int rewarMoney;//奖励+
    public int penalityMoney;//惩罚-
    [Header("关于移动到目标的下标")]
    public int moveToBoardIndex = -1;
    [Header("关于群体收钱")]
    public bool collectFromPlayer;
    [Header("关于监狱")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("关于街道维修")]
    public bool streetRepairs;
    public int streetRepairsHouse;
    public int streetRepairsHotel;
}
