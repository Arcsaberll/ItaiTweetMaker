using UnityEngine;

public class Player : MonoBehaviour
{
    private int playerNum;
    private string playerName;

    public Player(int num, string name)
    {
        playerNum = num;
        playerName = name;
    }

    public int GetPlayerNum()
    {
        return playerNum;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
