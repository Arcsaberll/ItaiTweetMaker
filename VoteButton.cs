using UnityEngine;

public class VoteButton : MonoBehaviour
{
    public GameManager gameManager;
    public int targetPlayerIndex; // 投票対象のプレイヤーインデックス

    public void OnVote()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager が設定されていません");
            return;
        }

        gameManager.SubmitVote(targetPlayerIndex);
    }
}
