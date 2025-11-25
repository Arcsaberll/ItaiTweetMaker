using UnityEngine;
using UnityEngine.UI; // Textを使う場合
// using TMPro; // ← TextMeshProを使う場合はこちらを有効化

public class CardDisplay : MonoBehaviour
{
    [Header("表示するカードデータ")]
    public TweetCard cardData;

    [Header("UI要素")]
    public Text descriptionText; 
    // public TextMeshProUGUI descriptionText; // ← TextMeshProの場合はこちら

    void Start()
    {
        if (cardData != null && descriptionText != null)
        {
            descriptionText.text = cardData.cardText;
        }
    }
}