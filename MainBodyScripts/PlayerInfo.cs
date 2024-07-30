using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText; //名字
    [SerializeField] TMP_Text playerCashText; //钱
    [SerializeField] GameObject activePlayerArrow;
    public void SetPlayerName(string newName)
    {
        playerNameText.text = "名称:" + newName;
    }
    public void SetPlayerCash(int currentCash)
    {
        playerCashText.text = "$" + currentCash;
    }
    public void SetPlayerBankrupt()
    {
        playerCashText.text = "破产了";
    }
    public void SetPlayerAndCash(string newName, int currentCash)
    {
        SetPlayerName(newName);
        SetPlayerCash(currentCash);
    }
    public void SetArrow(Color color)
    {
        activePlayerArrow.GetComponent<Image>().color = color;
    }
    public void ActivateArrow(bool active)
    {
        activePlayerArrow.SetActive(active);
    }
}
