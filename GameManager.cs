using UnityEngine;
using UnityEngine.UI; // Textコンポーネントを使うために必要
using System.Collections.Generic; // List<T> を使うために必要

/// <summary>
/// ゲーム全体の管理クラス
/// 【役割】プレイヤー管理、カード管理、ツイート・投票の処理を統括
/// 【主要機能】
/// - プレイヤーデータの管理（名前、手札、ツイート、スコア）
/// - 山札の管理（カードのロード、シャッフル、配布）
/// - ツイート作成フェーズの管理（カード配布、ツイート提出）
/// - 投票フェーズの管理（投票受付、スコア計算）
/// - ゲームのリセット処理
/// 【連携】GameFlowManagerと連携してフェーズ遷移を制御
/// </summary>
public class GameManager : MonoBehaviour
{
    // === データ管理 ===
    private List<TweetCard> deckCards = new List<TweetCard>();    // 山札（全カード）
    private List<PlayerData> players = new List<PlayerData>();    // 全プレイヤーのデータ
    private int currentPlayerIndex = 0;                           // 現在のプレイヤー番号

    // === 外部参照 ===
    private GameFlowManager gameFlowManager;                      // フェーズ管理クラス

    // === 設定 ===
    public bool autoRunGameOnStart = false;                       // 自動テスト実行フラグ（デバッグ用）
    
    private bool isSetupUIUsed = false;                          // セットアップUI使用フラグ
    public int selectedPlayerCount = 3;                          // プレイヤー人数（デフォルト3人）
    
    // === UI参照 ===
    public PostPanelUI postPanel;                                // ツイート作成パネル
    public TextSpawner spawner;                                  // カード生成クラス
    
    /// <summary>
    /// 初期化処理
    /// GameFlowManagerの取得、山札のロード、ゲーム開始
    /// </summary>
    void Start()
    {
        // GameFlowManager を取得
        gameFlowManager = GetComponent<GameFlowManager>();
        if (gameFlowManager == null)

        {
            gameFlowManager = FindObjectOfType<GameFlowManager>();
        }

        // Resources/TweetCards から全カードをロード
        LoadDeckFromResources();
        
        // セットアップUIが使用されていない場合のみ、ここでゲーム初期化
        if (!isSetupUIUsed)
        {
            InitializeGame();
        }

        // テスト用: 自動実行
        if (autoRunGameOnStart && !isSetupUIUsed)
        {
            Invoke("RunFullGameAutomatic", 2f); // 2秒後に実行
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 小さい GameManager から統合したリセット操作
    public void OnResetButton()
    {
        if (spawner != null)
        {
            spawner.SpawnFromSavedHand();
        }
        else
        {
            Debug.LogWarning("GameManager.OnResetButton: TextSpawner (spawner) が割り当てられていません");
        }

        if (postPanel != null)
        {
            postPanel.ResetPanel();
        }
        else
        {
            Debug.LogWarning("GameManager.OnResetButton: PostPanel (postPanel) が割り当てられていません");
        }
    }

    // PostButtonが押されたときの処理
    public void OnPostButton()
    {
        // tuerのテキストを取得
        GameObject postPanelObj = GameObject.Find("PostPanel");
        if (postPanelObj != null)
        {
            Transform tuerTransform = postPanelObj.transform.Find("tuer");
            if (tuerTransform != null)
            {
                Text tuerText = tuerTransform.GetComponent<Text>();
                if (tuerText != null)
                {
                    // 元のテキストをデバッグ出力
                    Debug.Log($"OnPostButton: 元のテキスト='{tuerText.text}' (長さ: {tuerText.text.Length})");
                    
                    // 初期メッセージを除外してツイートを取得
                    string tweet = tuerText.text.Trim();
                    Debug.Log($"OnPostButton: Trim後='{tweet}' (長さ: {tweet.Length})");
                    
                    if (tweet == "what's happeninig?" || tweet == "what's happening?")
                    {
                        Debug.Log("OnPostButton: 初期メッセージを検出、空文字列に変換");
                        tweet = ""; // 初期メッセージの場合は空文字列として扱う
                    }
                    
                    Debug.Log($"OnPostButton: SubmitTweetに渡すツイート='{tweet}'");
                    
                    // SubmitTweet呼び出し前に、これが最後のプレイヤーかどうかを判定
                    // (SubmitTweet内で投票フェーズに遷移するとcurrentPlayerIndexが0にリセットされるため)
                    bool isLastPlayer = (currentPlayerIndex >= players.Count - 1);
                    
                    // SubmitTweetを使用してツイートを保存し、次のプレイヤーへ移動
                    SubmitTweet(tweet);
                    
                    // 最後のプレイヤーでなければ次のプレイヤーのツイート作成画面を表示
                    if (!isLastPlayer)
                    {
                        // PostPanelをリセット(次のプレイヤー用)
                        if (postPanel != null)
                        {
                            postPanel.ResetPanel();
                        }
                        
                        if (gameFlowManager != null)
                        {
                            gameFlowManager.ShowTweetingUI();
                        }
                    }
                    else
                    {
                        Debug.Log("OnPostButton: 全プレイヤーのツイート作成が完了しました");
                    }
                }
                else
                {
                    Debug.LogWarning("tuerにTextコンポーネントがありません");
                }
            }
            else
            {
                Debug.LogWarning("PostPanel内にtuerが見つかりません");
            }
        }
        else
        {
            Debug.LogWarning("PostPanelが見つかりません");
        }
    }

    private void LoadDeckFromResources()
    {
        TweetCard[] cards = Resources.LoadAll<TweetCard>("TweetCards");
        Debug.Log($"Resources から {cards.Length} 枚のカードをロードしました");
        deckCards.AddRange(cards);
        ShuffleDeck();
        
        // シャッフル完了後のテスト出力
        Debug.Log($"=== シャッフル完了: 合計 {deckCards.Count} 枚のカード ===");
        for (int i = 0; i < deckCards.Count; i++)
        {
            Debug.Log($"[{i}] {deckCards[i].cardType} - {deckCards[i].cardText}");
        }
    }

    private void ShuffleDeck()
    {
        for (int i = deckCards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = deckCards[i];
            deckCards[i] = deckCards[randomIndex];
            deckCards[randomIndex] = temp;
        }
    }

    public TweetCard DrawCard()
    {
        if (deckCards.Count > 0)
        {
            TweetCard card = deckCards[0];
            deckCards.RemoveAt(0);
            return card;
        }
        return null;
    }

    // ユーザーが確定したツイートを受け取るためのメソッド
    public void SubmitTweet(string tweet)
    {
        if (string.IsNullOrEmpty(tweet))
        {
            Debug.LogWarning("ツイートが空です");
            return;
        }

        // 現在のプレイヤーにツイートを格納
        PlayerData currentPlayer = players[currentPlayerIndex];
        currentPlayer.tweetText = tweet;
        Debug.Log($"[{currentPlayer.playerName}] ツイート投稿: {tweet}");

        // 次のプレイヤーに進む
        MoveToNextPlayer();
    }

    // プレイヤーをツイート作成フェーズで回す
    public PlayerData GetCurrentPlayer()
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
        {
            return players[currentPlayerIndex];
        }
        return null;
    }
    
    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }
    
    public List<PlayerData> GetAllPlayers()
    {
        return players;
    }

    public bool IsAllPlayersTweeted()
    {
        return currentPlayerIndex >= players.Count;
    }

    // プレイヤーリストを取得（UIから参照される）
    public List<PlayerData> GetPlayers()
    {
        return players;
    }

    private void MoveToNextPlayer()
    {
        currentPlayerIndex++;
        if (IsAllPlayersTweeted())
        {
            Debug.Log("\n===== 全プレイヤーのツイート提出完了 =====");
            PrintAllSubmittedTweets();
            
            // GameFlowManager に通知してフェーズ遷移
            if (gameFlowManager != null)
            {
                gameFlowManager.OnTweetingPhaseComplete();
            }
            else
            {
                // GameFlowManager がない場合は直接遷移
                StartVotingPhase();
            }
        }
        else
        {
            PlayerData nextPlayer = players[currentPlayerIndex];
            Debug.Log($"\n========================================");
            Debug.Log($"【{nextPlayer.playerName}さんのツイート作成フェーズです】");
            Debug.Log($"========================================\n");
        }
    }

    private void PrintAllSubmittedTweets()
    {
        Debug.Log("【全プレイヤーのツイート一覧】");
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log($"{players[i].playerName}: {players[i].tweetText}");
        }
        Debug.Log("====================================\n");
    }

    // ========== 投票フェーズ ==========

    private void StartVotingPhase()
    {
        currentPlayerIndex = 0; // インデックスをリセット
        Debug.Log("\n========================================");
        Debug.Log("投票フェーズ開始");
        Debug.Log("========================================\n");
        
        PlayerData firstVoter = players[currentPlayerIndex];
        Debug.Log($"【{firstVoter.playerName}さんの投票フェーズです】\n");
        PrintVotableTweets(currentPlayerIndex);
    }
    
    // GameFlowManagerから呼ばれる投票フェーズ開始(UI表示は別途行う)
    public void StartVotingPhaseFromUI()
    {
        currentPlayerIndex = 0; // インデックスをリセット
        Debug.Log($"GameManager: 投票フェーズ開始 (currentPlayerIndexを0にリセット)");
    }

    // 投票対象のツイート一覧を表示（自分のツイートは除外）
    private void PrintVotableTweets(int voterIndex)
    {
        Debug.Log($"【{players[voterIndex].playerName} が投票できるツイート】");
        for (int i = 0; i < players.Count; i++)
        {
            if (i != voterIndex) // 自分以外
            {
                Debug.Log($"[{i}] {players[i].playerName}: {players[i].tweetText}");
            }
        }
    }

    // 投票を受け付ける
    public void SubmitVote(int votedPlayerIndex)
    {
        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            Debug.LogWarning("投票者のインデックスが不正です");
            return;
        }

        // 自分に投票できないようにチェック
        if (votedPlayerIndex == currentPlayerIndex)
        {
            Debug.LogWarning("自分に投票することはできません");
            return;
        }

        // 投票者が存在するかチェック
        if (votedPlayerIndex < 0 || votedPlayerIndex >= players.Count)
        {
            Debug.LogWarning("投票対象のプレイヤーが見つかりません");
            return;
        }

        // 投票記録を保存
        PlayerData voter = players[currentPlayerIndex];
        PlayerData votedPlayer = players[votedPlayerIndex];
        voter.votedTo.Add(votedPlayer.playerName);

        // スコア加算
        votedPlayer.score++;
        Debug.Log($"[{voter.playerName}] が [{votedPlayer.playerName}] に投票しました");
        Debug.Log($"{players[votedPlayerIndex].playerName} のスコア: {players[votedPlayerIndex].score}\n");

        // 次の投票者へ
        MoveToNextVoter();
    }

    private void MoveToNextVoter()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
        {
            // 全投票完了
            Debug.Log("\n===== 投票フェーズ完了 =====");
            PrintVotingResults();
            PrintFinalResults();
            
            // GameFlowManager に通知してフェーズ遷移
            if (gameFlowManager != null)
            {
                gameFlowManager.OnVotingPhaseComplete();
            }
            else
            {
                // GameFlowManager がない場合は直接遷移
                StartResultPhase();
            }
        }
        else
        {
            // 次の投票者の投票画面を表示
            PlayerData nextVoter = players[currentPlayerIndex];
            Debug.Log($"\n========================================");
            Debug.Log($"【{nextVoter.playerName}さんの投票フェーズです】");
            Debug.Log($"========================================\n");
            PrintVotableTweets(currentPlayerIndex);
            
            // 次のプレイヤーの投票UIを表示
            if (gameFlowManager != null)
            {
                gameFlowManager.ShowVotingUI();
            }
        }
    }

    // 投票結果を表示
    private void PrintVotingResults()
    {
        Debug.Log("【投票結果一覧】");
        for (int i = 0; i < players.Count; i++)
        {
            PlayerData player = players[i];
            if (player.votedTo.Count > 0)
            {
                Debug.Log($"{player.playerName} が投票した先:");
                foreach (var votedPlayerName in player.votedTo)
                {
                    Debug.Log($"  → {votedPlayerName}");
                }
            }
            else
            {
                Debug.Log($"{player.playerName}: 投票なし");
            }
        }
        Debug.Log("====================================\n");
    }

    // 最終結果を表示
    private void PrintFinalResults()
    {
        Debug.Log("\n========================================");
        Debug.Log("【投票フェーズ終了 - スコア集計】");
        Debug.Log("========================================");
        
        // スコアに基づいてプレイヤーをソート
        List<PlayerData> sortedPlayers = new List<PlayerData>(players);
        sortedPlayers.Sort((a, b) => b.score.CompareTo(a.score)); // 降順でソート

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            Debug.Log($"{i + 1}位: {sortedPlayers[i].playerName} (スコア: {sortedPlayers[i].score} 点)");
        }
        Debug.Log("========================================\n");

        // 結果発表フェーズへ遷移
        StartResultPhase();
    }

    // ========== 結果発表フェーズ ==========

    private void StartResultPhase()
    {
        Debug.Log("\n========================================");
        Debug.Log("ツイート発表フェーズ - 結果");
        Debug.Log("========================================\n");

        // 勝者を決定
        PlayerData winner = GetWinner();
        if (winner != null)
        {
            Debug.Log($"🎉 勝利者: {winner.playerName} 🎉");
            Debug.Log($"ツイート: {winner.tweetText}");
            Debug.Log($"スコア: {winner.score}\n");

            PrintDetailedResults();
        }
        else
        {
            Debug.LogWarning("勝者を決定できませんでした");
        }

        Debug.Log("========================================\n");
        
        // GameFlowManager に通知
        if (gameFlowManager != null)
        {
            gameFlowManager.OnResultPhaseComplete();
        }
        else
        {
            Debug.Log("ゲーム終了\n");
        }
    }

    // 勝者を取得
    private PlayerData GetWinner()
    {
        if (players.Count == 0) return null;

        PlayerData winner = players[0];
        foreach (PlayerData player in players)
        {
            if (player.score > winner.score)
            {
                winner = player;
            }
        }
        return winner;
    }

    // 詳細な結果を表示
    private void PrintDetailedResults()
    {
        Debug.Log("\n========================================");
        Debug.Log("【最終ランキング】");
        Debug.Log("========================================");
        
        // スコアに基づいてプレイヤーをソート
        List<PlayerData> sortedPlayers = new List<PlayerData>(players);
        sortedPlayers.Sort((a, b) => b.score.CompareTo(a.score)); // 降順でソート

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            Debug.Log($"\n{i + 1}位: {sortedPlayers[i].playerName}");
            Debug.Log($"  ツイート: {sortedPlayers[i].tweetText}");
            Debug.Log($"  スコア: {sortedPlayers[i].score} 点");
        }
        Debug.Log("\n========================================\n");
    }

    // セットアップUIから呼び出され、指定されたプレイヤー名でゲーム初期化
    public void InitializeGameWithPlayers(List<string> playerNames)
    {
        isSetupUIUsed = true;
        
        // 既存のプレイヤーをクリア（Start()で自動生成されたプレイヤーを削除）
        players.Clear();
        currentPlayerIndex = 0;
        
        int playerCount = playerNames.Count;
        
        Debug.Log($"=== InitializeGameWithPlayers 開始 ===");
        Debug.Log($"受け取ったプレイヤー名: {playerCount}人");
        for (int i = 0; i < playerNames.Count; i++)
        {
            Debug.Log($"  [{i}] {playerNames[i]}");
        }

        // 指定されたプレイヤーを生成
        for (int i = 0; i < playerCount; i++)
        {
            PlayerData player = new PlayerData(playerNames[i]);
            players.Add(player);
            Debug.Log($"プレイヤー作成: {player.playerName}");
        }

        // 全プレイヤーにカードを配布
        DealCardsToAllPlayers();

        // テスト出力
        PrintPlayersHand();

        // ツイート作成フェーズ開始
        StartTweetingPhase();
        
        Debug.Log($"=== InitializeGameWithPlayers 完了 ===");
    }

    private void InitializeGame()
    {
        // ドロップダウンで選択されたプレイヤー数を使用（範囲外ならランダム）
        int playerCount = selectedPlayerCount;
        if (playerCount < 3 || playerCount > 6)
        {
            playerCount = Random.Range(3, 7); // 3～6
        }
        Debug.Log($"ゲーム開始: {playerCount} 人でゲームを開始します");

        // プレイヤーを生成
        for (int i = 0; i < playerCount; i++)
        {
            PlayerData player = new PlayerData($"プレイヤー{i + 1}");
            players.Add(player);
        }

        // 全プレイヤーにカードを配布
        DealCardsToAllPlayers();

        // テスト出力
        PrintPlayersHand();

        // ツイート作成フェーズ開始
        StartTweetingPhase();
    }

    private void StartTweetingPhase()
    {
        currentPlayerIndex = 0;
        Debug.Log("\n========================================");
        Debug.Log("ツイート作成フェーズ開始");
        Debug.Log("========================================\n");
        Debug.Log($"*** {players[currentPlayerIndex].playerName} のターン開始 ***\n");
    }

    private void DealCardsToAllPlayers()
    {
        // 各プレイヤーに必要なカードの枚数
        int cardsPerPlayer = 8; // Opening 2 + Middle 2 + Ending 2 + (Opening, Middle, Ending から) 2

        foreach (PlayerData player in players)
        {
            // タイプ別に 2 枚ずつ配布
            DealCardsByType(player, CardType.Opening, 2);
            DealCardsByType(player, CardType.Middle, 2);
            DealCardsByType(player, CardType.Ending, 2);
            
            // Opening, Middle, Ending を合わせた中からランダムに 2 枚
            for (int i = 0; i < 2; i++)
            {
                TweetCard card = DrawRandomCardFromAllTypes();
                if (card != null)
                {
                    player.AddCard(card);
                }
            }
        }
    }

    private void DealCardsByType(PlayerData player, CardType type, int count)
    {
        int dealt = 0;
        int attempts = 0;
        int maxAttempts = deckCards.Count; // 無限ループ防止

        while (dealt < count && attempts < maxAttempts)
        {
            for (int i = deckCards.Count - 1; i >= 0; i--)
            {
                if (deckCards[i].cardType == type)
                {
                    TweetCard card = deckCards[i];
                    player.AddCard(card);
                    deckCards.RemoveAt(i);
                    dealt++;
                    break;
                }
            }
            attempts++;
        }

        if (dealt < count)
        {
            Debug.LogWarning($"{player.playerName} に {type} カードを {count} 枚配布できませんでした (配布済み: {dealt} 枚)");
        }
    }

    private TweetCard DrawRandomCard()
    {
        if (deckCards.Count > 0)
        {
            int randomIndex = Random.Range(0, deckCards.Count);
            TweetCard card = deckCards[randomIndex];
            deckCards.RemoveAt(randomIndex);
            return card;
        }
        return null;
    }

    private TweetCard DrawRandomCardFromAllTypes()
    {
        // Opening, Middle, Ending のカードをフィルタリング
        List<TweetCard> availableCards = new List<TweetCard>();
        foreach (TweetCard card in deckCards)
        {
            if (card.cardType == CardType.Opening || 
                card.cardType == CardType.Middle || 
                card.cardType == CardType.Ending)
            {
                availableCards.Add(card);
            }
        }

        if (availableCards.Count > 0)
        {
            TweetCard selectedCard = availableCards[Random.Range(0, availableCards.Count)];
            deckCards.Remove(selectedCard);
            return selectedCard;
        }

        return null;
    }

    private void PrintPlayersHand()
    {
        Debug.Log("\n========================================");
        Debug.Log("ゲーム開始時の手札配布 - 完了");
        Debug.Log("========================================");
        Debug.Log($"総プレイヤー数: {players.Count} 人\n");
        
        foreach (PlayerData player in players)
        {
            player.DebugPrintHand();
            Debug.Log("----");
        }
        
        Debug.Log($"残りデッキ: {deckCards.Count} 枚");
        Debug.Log("========================================\n");
    }

    // ========== テスト用: 自動ゲーム実行 ==========

    // UIなしで全ゲームを自動実行（テスト用）
    public void RunFullGameAutomatic()
    {
        Debug.Log("\n【テスト開始: 全ゲーム自動実行】\n");
        
        // ツイート作成フェーズを自動実行
        AutoSubmitAllTweets();
        
        // 投票フェーズを自動実行
        AutoVoteForTesting();
    }

    // 全プレイヤーのツイートを自動生成・提出
    private void AutoSubmitAllTweets()
    {
        Debug.Log("\n【ツイート作成フェーズ自動実行】");
        
        for (int i = 0; i < players.Count; i++)
        {
            string autoTweet = GenerateAutoTweet(i);
            SubmitTweet(autoTweet);
        }
    }

    // ランダムなツイートを生成
    private string GenerateAutoTweet(int playerIndex)
    {
        string[] openings = { "おはよう", "こんにちは", "こんばんは", "ただいま", "いってきます" };
        string[] middles = { "今日も", "今は", "これから", "さっき", "ずっと" };
        string[] endings = { "頑張ろう", "楽しいな", "最高だ", "疲れた", "嬉しい" };

        string opening = openings[Random.Range(0, openings.Length)];
        string middle = middles[Random.Range(0, middles.Length)];
        string ending = endings[Random.Range(0, endings.Length)];

        return $"{opening} {middle} {ending}";
    }

    // 全プレイヤーの投票を自動実行
    public void AutoVoteForTesting()
    {
        Debug.Log("\n【投票フェーズ自動実行】\n");
        
        while (!IsAllVotersVoted())
        {
            PlayerData voter = players[currentPlayerIndex];
            Debug.Log($"========================================");
            Debug.Log($"【{voter.playerName}さんの投票フェーズです】");
            Debug.Log($"========================================\n");
            
            // ランダムに投票対象を選ぶ（自分以外）
            int voterIndex = currentPlayerIndex;
            int targetIndex;
            do
            {
                targetIndex = Random.Range(0, players.Count);
            } while (targetIndex == voterIndex);

            PlayerData votedPlayer = players[targetIndex];
            Debug.Log($"【{votedPlayer.playerName}: {votedPlayer.tweetText}】 に投票します\n");
            
            SubmitVote(targetIndex);
        }
    }

    private bool IsAllVotersVoted()
    {
        return currentPlayerIndex >= players.Count;
    }
    
    // ゲームをリセット
    public void ResetGame()
    {
        Debug.Log("=== ゲームリセット ===");
        
        // プレイヤーデータをクリア
        players.Clear();
        currentPlayerIndex = 0;
        
        // セットアップフラグをリセット
        isSetupUIUsed = false;
        
        // 山札をリセット
        deckCards.Clear();
        LoadDeckFromResources();
        Debug.Log($"山札をリセットしました: {deckCards.Count} 枚");
        
        // PostPanelのテキストをリセット
        if (postPanel != null)
        {
            if (postPanel.postPanelText != null)
            {
                postPanel.postPanelText.text = "what's happeninig?\n";
            }
            
            // ドロップエリアをクリア
            if (postPanel.tweetDropArea != null)
            {
                postPanel.tweetDropArea.ClearTweet();
            }
            
            // ボタン状態をリセット
            if (postPanel.resetButton != null) postPanel.resetButton.interactable = true;
            if (postPanel.postButton != null) postPanel.postButton.interactable = false;
        }
        
        // TextSpawnerの手札をクリア
        if (spawner != null)
        {
            // SpawnArea内のカードを全て削除
            if (spawner.spawnArea != null)
            {
                foreach (Transform child in spawner.spawnArea.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        Debug.Log("ゲームがリセットされました");
    }
}
