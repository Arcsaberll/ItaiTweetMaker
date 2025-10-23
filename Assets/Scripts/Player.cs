using UnityEngine;

public class Player(int num, string name) : MonoBehaviour
{
    private string playerName = name;
    private int score = 0;

    public int GetPlayerNum()
    {
        return num;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void ResetScore()
    {
        score = 0;
    }
}
