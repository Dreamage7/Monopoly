using TMPro;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    Player newPlayer;

    private void OnEnable()
    {
        ClearMessage();
        GameManager.OnUpdateMassage += ReceiveMessage;
        Player.OnUpdateMassage += ReceiveMessage;
        MonopolyNode.OnUpdateMassage += ReceiveMessage;
        MaybeTradingSystem.OnUpdateMassage += ReceiveMessage;
    }

    private void OnDisable()
    {
        GameManager.OnUpdateMassage -= ReceiveMessage;
        Player.OnUpdateMassage -= ReceiveMessage;
        MonopolyNode.OnUpdateMassage -= ReceiveMessage;
        MaybeTradingSystem.OnUpdateMassage -= ReceiveMessage;
    }

    void ReceiveMessage(string _message,Player currentPlayer)
    {
        //Player player = currentPlayer;
        messageText.text = _message;
        //if (newPlayer == player)
        //{
        //    messageText.text = messageText.text + ";" + _message;
        //}
        //else
        //{
        //    newPlayer = player;
        //    messageText.text = _message;
        //}
        Invoke("ClearMessage",2f);
    }

    void ClearMessage()
    {
        messageText.text = "";
    }
}
