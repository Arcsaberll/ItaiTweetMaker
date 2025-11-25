using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// カードタイプ定義
/// Opening: ツイートの書き出し、Middle: 本文、Ending: 締めくくり
/// </summary>
public enum CardType
{
    Opening,    // ツイートの冒頭（書き出し）
    Middle,     // 中盤（本文）
    Ending      // 終わり（締めくくり）
}

/// <summary>
/// ツイートカードデータクラス（ScriptableObject）
/// 【役割】ツイートを構成するカードの内容とタイプを保持
/// 【主要機能】
/// - カードIDの管理
/// - カードテキストの保持
/// - カードタイプの分類（Opening/Middle/Ending）
/// 【使用方法】
/// 1. Project内で右クリック→Create→TweetGame→TweetCardで作成
/// 2. InspectorでcardID、cardText、cardTypeを設定
/// 3. Resources/TweetCardsフォルダに配置
/// 4. GameManagerのLoadDeckFromResources()で自動読み込み
/// </summary>
[CreateAssetMenu(fileName = "TweetCard", menuName = "TweetGame/TweetCard")]
public class TweetCard : ScriptableObject
{
    public string cardID;       // カードの一意識別子（例: "opening_01"）
    
    [TextArea]
    public string cardText;     // カードのテキスト内容（複数行可）
    
    public CardType cardType;   // カードのタイプ（Opening/Middle/Ending）
}
