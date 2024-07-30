using System.Collections.Generic;
using UnityEngine;

public static class GameSetting
{
    public static List<Setting> settingsList = new List<Setting>();
    public static int startGameMoney = 0;
    public static void AddSetting(Setting setting)
    {
        settingsList.Add(setting);
    }

    public static void ClearSetting()
    {
        settingsList.Clear();
    }
}

public class Setting
{
    public string playerName;
    public int playerType;
    public int colorType;

    public Setting(string Name, int Type, int color)
    {
        playerName = Name;
        playerType = Type;
        colorType = color;
    }
}
