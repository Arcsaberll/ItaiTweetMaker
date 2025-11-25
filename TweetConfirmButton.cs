using UnityEngine;

public class TweetConfirmButton : MonoBehaviour
{
    public TweetDropArea dropArea;
    public GameManager gameManager;

    public void OnConfirmTweet()
    {
        string tweet = dropArea.GetTweetText();
        if (string.IsNullOrEmpty(tweet))
        {
            Debug.Log("ツイートが空です！");
            return;
        }

        // ツイート提出
        gameManager.SubmitTweet(tweet);

        // UI をリセット（次のプレイヤーのために）
        dropArea.ClearTweet();
    }
}
