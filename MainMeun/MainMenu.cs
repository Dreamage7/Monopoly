using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Serializable]
    public class PlayerSelect
    {
        public TMP_InputField nameText;
        public TMP_Dropdown playerType;
        public TMP_Dropdown colorType;
        public Toggle toggle;
    }
    [SerializeField] PlayerSelect[] playerSelect;
    [SerializeField] Button startButton;
    [SerializeField] Button EndButton;
    [SerializeField] TMP_InputField startMoney;

    public void StartButton()
    {
        int startGameMoney = Int32.Parse(startMoney.text);
        GameSetting.startGameMoney = startGameMoney;
        foreach (var player in playerSelect)
        {
            if (player.toggle.isOn)
            {
                GameSetting.AddSetting(new Setting(player.nameText.text,player.playerType.value,player.colorType.value));
            }
        }
        SceneManager.LoadScene("Game");
    }
}
