using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TweetDropArea : MonoBehaviour, IDropHandler
{
    public List<TweetCard> composedTweet = new List<TweetCard>();

    public void OnDrop(PointerEventData eventData)
    {
        TweetCardUI droppedCard = eventData.pointerDrag.GetComponent<TweetCardUI>();
        if (droppedCard != null)
        {
            droppedCard.transform.SetParent(transform); // ツイートエリアに移動
            composedTweet.Add(droppedCard.cardData);
        }
    }

    public string GetTweetText()
    {
        string tweet = "";
        foreach (var card in composedTweet)
        {
            tweet += card.cardText + " ";
        }
        return tweet.Trim();
    }

    public void ClearTweet()
    {
        // 配置されたカードを削除
        composedTweet.Clear();
        Debug.Log("ツイートエリアをクリアしました");
    }
}
