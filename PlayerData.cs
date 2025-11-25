using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// プレイヤーデータクラス
/// 【役割】各プレイヤーの状態管理（名前、スコア、手札、ツイート、投票履歴）
/// 【主要機能】
/// - プレイヤー名とスコアの保持
/// - 手札の管理（全体 + タイプ別：Opening/Middle/Ending）
/// - ツイート本文の保存
/// - 投票履歴の記録
/// 【使用方法】
/// - GameManagerが各プレイヤーのデータを保持
/// - カード配布時にAddCard()でタイプ別に自動分類
/// - 投票フェーズでスコア加算
/// </summary>
public class PlayerData
{
    // === 基本情報 ===
    public string playerName;                           // プレイヤー名
    public int score;                                   // 得票数（投票フェーズで加算）
    
    // === 手札管理 ===
    public List<TweetCard> hand = new List<TweetCard>();                // 全手札
    public List<TweetCard> openingCards = new List<TweetCard>();        // Opening（書き出し）カード
    public List<TweetCard> middleCards = new List<TweetCard>();         // Middle（本文）カード
    public List<TweetCard> endingCards = new List<TweetCard>();         // Ending（締めくくり）カード
    
    // === ツイート ===
    public string tweetText;                            // 作成したツイート本文
    
    // === 投票履歴 ===
    public List<string> votedTo = new List<string>();   // このプレイヤーが投票した先（プレイヤー名）

    /// <summary>
    /// コンストラクタ
    /// プレイヤー名を設定し、スコアを0で初期化
    /// </summary>
    /// <param name="name">プレイヤー名</param>
    public PlayerData(string name)
    {
        playerName = name;
        score = 0;
    }

    /// <summary>
    /// カードをタイプ別に追加
    /// handに追加すると同時に、カードタイプに応じてタイプ別リストにも追加
    /// </summary>
    /// <param name="card">追加するカード</param>
    public void AddCard(TweetCard card)
    {
        hand.Add(card);
        
        // カードタイプに応じて分類
        switch (card.cardType)
        {
            case CardType.Opening:
                openingCards.Add(card);
                break;
            case CardType.Middle:
                middleCards.Add(card);
                break;
            case CardType.Ending:
                endingCards.Add(card);
                break;
        }
    }

    /// <summary>
    /// 手札のデバッグ表示
    /// タイプ別に整理して手札の内容をコンソールに出力
    /// </summary>
    public void DebugPrintHand()
    {
        Debug.Log($"【{playerName}の手札】(全 {hand.Count} 枚)");
        
        Debug.Log($"Opening: {openingCards.Count}枚");
        foreach (var card in openingCards)
        {
            Debug.Log($"  - {card.cardText}");
        }
        
        Debug.Log($"Middle: {middleCards.Count}枚");
        foreach (var card in middleCards)
        {
            Debug.Log($"  - {card.cardText}");
        }
        
        Debug.Log($"Ending: {endingCards.Count}枚");
        foreach (var card in endingCards)
        {
            Debug.Log($"  - {card.cardText}");
        }

        // ランダムに配布されたカード（Opening, Middle, Ending 以外）
        List<TweetCard> randomCards = new List<TweetCard>();
        foreach (var card in hand)
        {
            if (!openingCards.Contains(card) && !middleCards.Contains(card) && !endingCards.Contains(card))
            {
                randomCards.Add(card);
            }
        }

        if (randomCards.Count > 0)
        {
            Debug.Log($"ランダム配布: {randomCards.Count}枚");
            foreach (var card in randomCards)
            {
                Debug.Log($"  - [{card.cardType}] {card.cardText}");
            }
        }
    }
}
