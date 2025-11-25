using UnityEngine;
using UnityEngine.UI;

public class PostPanelUI : MonoBehaviour
{
    [Header("UI参照")]
    public Text postPanelText;          // PostPanel（作成中ツイート表示用Text）
    public GameObject spawnAreaPanel;   // Panel/spawnarea（手札表示エリア）
    public TextSpawner textSpawner;     // 手札生成用TextSpawner
    public Button resetButton;          // ResetButton
    public Button postButton;           // PostButton

    [Header("Optional: ドロップエリア")]
    public TweetDropArea tweetDropArea; // もしPostPanelがTweetDropAreaなら割当

    public void ResetPanel()
    {
        // 作成中ツイート表示を初期メッセージに戻す
        if (postPanelText != null)
        {
            postPanelText.text = "what's happeninig?\n";
        }

        // TweetDropAreaがあればクリア
        if (tweetDropArea != null)
        {
            tweetDropArea.ClearTweet();
        }

        // 手札を再生成
        if (textSpawner != null)
        {
            textSpawner.SpawnFromSavedHand();
        }

        // ボタン状態をリセット
        if (resetButton != null) resetButton.interactable = true;
        if (postButton != null) postButton.interactable = false; // カードがない状態なので無効化

        Debug.Log("PostPanelUI: ツイート作成パネルをリセットしました");
    }
}