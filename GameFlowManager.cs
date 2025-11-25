using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームフェーズ定義
/// Setup: プレイヤー設定、Tweeting: ツイート作成、Voting: 投票、Result: 結果表示
/// </summary>
public enum GamePhase
{
    Setup,      // プレイヤー設定フェーズ
    Tweeting,   // ツイート作成フェーズ
    Voting,     // 投票フェーズ
    Result      // 結果表示フェーズ
}

/// <summary>
/// ゲームフロー管理クラス
/// 【役割】ゲーム全体のフェーズ遷移とUI表示を管理
/// 【主要機能】
/// - ゲームフェーズの管理と切り替え（Setup → Tweeting → Voting → Result）
/// - 各フェーズのUI表示/非表示制御
/// - 投票候補ツイートの表示とランキング生成
/// - BGMの自動切り替え
/// - リスタート/終了処理
/// 【連携】GameManagerと連携してゲームロジックを実行
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public GamePhase currentPhase;                         // 現在のゲームフェーズ

    // === 外部参照 ===
    private GameManager gameManager;                       // ゲームロジック管理クラス

    // === UIパネル参照 ===
    [SerializeField] private GameObject setupUIPanel;      // プレイヤー設定画面
    [SerializeField] private GameObject tweetingUIPanel;   // ツイート作成画面
    [SerializeField] private GameObject votingUIPanel;     // 投票画面
    [SerializeField] private GameObject resultUIPanel;     // 結果画面
    
    // === ツイート作成フェーズUI ===
    [SerializeField] private GameObject tweetingStartPanel;              // ターン開始通知パネル
    [SerializeField] private Text tweetingStartPanelPlayerNameText;      // プレイヤー名表示
    [SerializeField] private Button tweetingStartButton;                 // 開始ボタン
    [SerializeField] private Text tweetingUIPanelPlayerNameText;         // 作成画面プレイヤー名
    
    // === 投票フェーズUI ===
    [SerializeField] private GameObject votingStartPanel;                // 投票ターン開始パネル
    [SerializeField] private Text votingStartPanelPlayerNameText;        // 投票者名表示
    [SerializeField] private Button votingStartButton;                   // 投票開始ボタン
    [SerializeField] private Transform tweetListContainer;               // ツイート選択肢コンテナ
    [SerializeField] private GameObject tweetItemPrefab;                 // ツイート項目プレハブ
    [SerializeField] private Button voteButton;                          // 投票確定ボタン
    
    // === 結果フェーズUI ===
    [SerializeField] private Transform resultRankingContainer;           // ランキング表示コンテナ
    [SerializeField] private GameObject rankingItemPrefab;               // ランキング項目プレハブ
    [SerializeField] private Button restartButton;                       // リスタートボタン
    [SerializeField] private Button quitButton;                          // 終了ボタン
    
    // === シーン設定 ===
    [Header("シーン設定")]
    [SerializeField] private bool hasSeperateTitle = false;              // タイトルシーン分離フラグ
    [SerializeField] private string titleSceneName = "TitleScene";       // タイトルシーン名
    
    // === 内部状態 ===
    private int selectedPlayerIndex = -1;                  // 投票で選択されたプレイヤー
    private GameObject selectedTweetItem = null;           // 選択中のツイートUI
    
    private BGMManager bgmManager;                         // BGM管理クラス

    /// <summary>
    /// 初期化処理
    /// GameManagerとBGMManagerの取得、UI要素の自動検索
    /// </summary>
    private void Awake()
    {
        // GameManager を取得
        gameManager = GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        // BGMManager を取得
        bgmManager = FindObjectOfType<BGMManager>();
        if (bgmManager == null)
        {
            Debug.LogWarning("BGMManager が見つかりません。BGMManagerオブジェクトをシーンに追加してください。");
        }

        // UI パネルへの参照を自動取得（Inspector未設定時の自動補完）
        if (setupUIPanel == null) setupUIPanel = GameObject.Find("SetupUIPanel");
        if (tweetingUIPanel == null) tweetingUIPanel = GameObject.Find("TweetingUIPanel");
        if (votingUIPanel == null) votingUIPanel = GameObject.Find("VotingUIPanel");
        if (resultUIPanel == null) resultUIPanel = GameObject.Find("ResultUIPanel");

        // ツイート作成フェーズの子要素を自動取得(Inspector未設定時)
        if (tweetingStartPanel == null)
        {
            tweetingStartPanel = GameObject.Find("TweetingStartPanel");
        }
        
        // 投票フェーズの子要素を自動取得(Inspector未設定時)
        if (votingStartPanel == null)
        {
            votingStartPanel = GameObject.Find("VotingStartPanel");
        }
        
        // TweetingStartPanel のプレイヤー名テキストを取得
        if (tweetingStartPanelPlayerNameText == null)
        {
            // TweetingStartPanel の子要素から検索
            if (tweetingStartPanel != null)
            {
                var texts = tweetingStartPanel.GetComponentsInChildren<Text>(true);
                foreach (var t in texts)
                {
                    if (t.name == "TweetingPlayerNameText")
                    {
                        tweetingStartPanelPlayerNameText = t;
                        break;
                    }
                }
            }
        }
        
        // TweetingUIPanel のプレイヤー名テキストを取得
        if (tweetingUIPanelPlayerNameText == null)
        {
            if (tweetingUIPanel != null)
            {
                var texts = tweetingUIPanel.GetComponentsInChildren<Text>(true);
                foreach (var t in texts)
                {
                    if (t.name == "TweetingPlayerNameText")
                    {
                        tweetingUIPanelPlayerNameText = t;
                        break;
                    }
                }
            }
        }
        
        // TweetingStartButton を取得
        if (tweetingStartButton == null)
        {
            // TweetingStartPanel の子要素から検索
            if (tweetingStartPanel != null)
            {
                tweetingStartButton = tweetingStartPanel.GetComponentInChildren<Button>(true);
            }
            // 見つからなければグローバル検索
            if (tweetingStartButton == null)
            {
                var go = GameObject.Find("TweetingStartButton");
                if (go != null) tweetingStartButton = go.GetComponent<Button>();
            }
        }
    }

    /// <summary>
    /// ゲーム開始処理
    /// 初期フェーズをセットアップに設定
    /// </summary>
    private void Start()
    {
        // 初期フェーズをセットアップに設定
        ChangePhase(GamePhase.Setup);
    }

    /// <summary>
    /// フェーズ切り替え処理
    /// 各フェーズのUI表示とBGM切り替えを実行
    /// </summary>
    /// <param name="nextPhase">遷移先のフェーズ</param>
    public void ChangePhase(GamePhase nextPhase)
    {
        currentPhase = nextPhase;
        Debug.Log($"フェーズ遷移: {currentPhase}");

        switch (currentPhase)
        {
            case GamePhase.Setup:
                ShowSetupUI();
                if (bgmManager != null) bgmManager.PlaySetupBGM();
                break;
            case GamePhase.Tweeting:
                ShowTweetingUI();
                if (bgmManager != null) bgmManager.PlayTweetingBGM();
                break;
            case GamePhase.Voting:
                // GameManagerの投票フェーズを開始してcurrentPlayerIndexをリセット
                if (gameManager != null)
                {
                    gameManager.StartVotingPhaseFromUI();
                }
                ShowVotingUI();
                if (bgmManager != null) bgmManager.PlayVotingBGM();
                break;
            case GamePhase.Result:
                ShowResultUI();
                if (bgmManager != null) bgmManager.PlayResultBGM();
                break;
        }
    }

    /// <summary>
    /// セットアップUI表示
    /// プレイヤー名・人数設定画面を表示
    /// </summary>
    void ShowSetupUI()
    {
        // すべてのUIを非表示にしてからセットアップUIを表示
        HideAllPanels();
        if (setupUIPanel != null)
        {
            setupUIPanel.SetActive(true);
            Debug.Log("セットアップUI表示");
        }
        else
        {
            Debug.LogWarning("SetupUIPanel が見つかりません");
        }
    }

    /// <summary>
    /// ツイート作成UI表示
    /// プレイヤー名表示、開始パネル、カード配布などを制御
    /// </summary>
    public void ShowTweetingUI()
    {
        Debug.Log("=== ShowTweetingUI 開始 ===");
        
        // すべてのUIを非表示
        HideAllPanels();

        // 現在のプレイヤー名を取得
        string playerName = "プレイヤー";
        if (gameManager != null)
        {
            var current = gameManager.GetCurrentPlayer();
            if (current != null)
            {
                playerName = current.playerName;
                Debug.Log($"現在のプレイヤー: {playerName}");
            }
            else
            {
                Debug.LogWarning("GetCurrentPlayer() が null を返しました");
            }
        }
        else
        {
            Debug.LogWarning("GameManager が null です");
        }

        // TweetingStartPanel を表示（プレイヤー名通知+開始ボタン）
        if (tweetingStartPanel != null)
        {
            Debug.Log($"TweetingStartPanel の状態: active={tweetingStartPanel.activeSelf}, name={tweetingStartPanel.name}");
            tweetingStartPanel.SetActive(true);
            Debug.Log($"TweetingStartPanel を表示しました (SetActive(true) 実行後: {tweetingStartPanel.activeSelf})");
        }
        else
        {
            Debug.LogWarning("TweetingStartPanel が null です！");
        }

        // TweetingStartPanel のプレイヤー名メッセージを設定（パネル表示後に設定）
        if (tweetingStartPanelPlayerNameText != null)
        {
            tweetingStartPanelPlayerNameText.text = $"{playerName}さんのツイート作成ターンです。";
            tweetingStartPanelPlayerNameText.enabled = true;
            tweetingStartPanelPlayerNameText.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            Debug.Log($"TweetingStartPanel PlayerNameText 更新: {tweetingStartPanelPlayerNameText.text}");
        }
        else
        {
            Debug.LogWarning("TweetingStartPanelPlayerNameText が null です！");
        }

        // TweetingStartButton のリスナー設定
        if (tweetingStartButton != null)
        {
            tweetingStartButton.gameObject.SetActive(true);
            tweetingStartButton.onClick.RemoveAllListeners();
            tweetingStartButton.onClick.AddListener(ShowTweetComposePanel);
            Debug.Log("TweetingStartButton のリスナー設定完了");
        }
        else
        {
            Debug.LogWarning("TweetingStartButton が null です！");
        }

        Debug.Log($"=== ShowTweetingUI 完了: [{playerName}] のターン開始 ===");
    }

    /// <summary>
    /// ツイート作成パネル表示
    /// 開始パネルを非表示にし、実際のカード操作画面を表示
    /// </summary>
    private void ShowTweetComposePanel()
    {
        // TweetingStartPanel を非表示
        if (tweetingStartPanel != null)
        {
            tweetingStartPanel.SetActive(false);
        }

        // TweetingUIPanel を表示（実際のツイート作成画面）
        if (tweetingUIPanel != null)
        {
            tweetingUIPanel.SetActive(true);
        }

        // TweetingUIPanel のプレイヤー名を設定
        if (tweetingUIPanelPlayerNameText != null && gameManager != null)
        {
            var current = gameManager.GetCurrentPlayer();
            if (current != null)
            {
                tweetingUIPanelPlayerNameText.text = current.playerName;
                Debug.Log($"TweetingUIPanel PlayerNameText 更新: {tweetingUIPanelPlayerNameText.text}");
            }
        }

        // 手札を表示（TextSpawnerによるカード生成）
        if (gameManager != null && gameManager.spawner != null)
        {
            gameManager.spawner.SpawnCurrentPlayerHand();
        }

        Debug.Log("ツイート作成パネル表示: カードを配置してください");
    }

    /// <summary>
    /// 投票UI表示
    /// 投票者名の表示と投票開始パネルの表示
    /// </summary>
    public void ShowVotingUI()
    {
        currentPhase = GamePhase.Voting;
        Debug.Log("投票フェーズを開始します");
        
        // すべてのUIを非表示
        HideAllPanels();

        // 現在のプレイヤー名を取得（投票者）
        string playerName = "プレイヤー";
        if (gameManager != null)
        {
            var current = gameManager.GetCurrentPlayer();
            if (current != null)
            {
                playerName = current.playerName;
                Debug.Log($"現在の投票者: {playerName}");
            }
        }

        // VotingStartPanel を表示（投票者名通知+開始ボタン）
        if (votingStartPanel != null)
        {
            votingStartPanel.SetActive(true);
            Debug.Log($"VotingStartPanel を表示しました");
        }
        else
        {
            Debug.LogWarning("VotingStartPanel が null です!");
        }

        // VotingStartPanel のプレイヤー名メッセージを設定
        if (votingStartPanelPlayerNameText != null)
        {
            votingStartPanelPlayerNameText.text = $"{playerName}さんの投票ターンです";
            Debug.Log($"VotingStartPanel PlayerNameText 更新: {votingStartPanelPlayerNameText.text}");
        }

        // VotingStartButton のリスナー設定
        if (votingStartButton != null)
        {
            votingStartButton.onClick.RemoveAllListeners();
            votingStartButton.onClick.AddListener(() =>
            {
                Debug.Log("投票開始ボタンが押されました");
                ShowVotingMainUI();
            });
        }
    }
    
    /// <summary>
    /// 投票メインUI表示
    /// VotingUIPanelを表示し、投票対象ツイート一覧を生成
    /// </summary>
    private void ShowVotingMainUI()
    {
        // VotingStartPanelを非表示
        if (votingStartPanel != null)
        {
            votingStartPanel.SetActive(false);
        }
        
        // VotingUIPanelを表示
        if (votingUIPanel != null)
        {
            votingUIPanel.SetActive(true);
            Debug.Log("VotingUIPanel を表示しました");
            
            // 投票対象のツイート一覧を表示
            DisplayVotableTweets();
        }
        else
        {
            Debug.LogWarning("VotingUIPanel が見つかりません");
        }
        
        // 投票ボタンを無効化（ツイート選択まで）
        if (voteButton != null)
        {
            voteButton.interactable = false;
        }
    }
    
    /// <summary>
    /// 投票可能ツイート一覧表示
    /// 自分以外のプレイヤーのツイートを選択肢として表示（匿名化）
    /// </summary>
    private void DisplayVotableTweets()
    {
        if (gameManager == null || tweetListContainer == null)
        {
            Debug.LogWarning("GameManager または TweetListContainer が null です");
            return;
        }
        
        // 既存のツイート項目をクリア
        foreach (Transform child in tweetListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // TweetListContainerにVerticalLayoutGroupを追加(なければ)
        var layoutGroup = tweetListContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = tweetListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        }
        
        // ContentSizeFitterを追加(なければ)
        var contentSizeFitter = tweetListContainer.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = tweetListContainer.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        var players = gameManager.GetAllPlayers();
        int currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
        
        Debug.Log($"=== 投票対象ツイート表示 ===");
        Debug.Log($"総プレイヤー数={players.Count}");
        Debug.Log($"現在の投票者インデックス={currentPlayerIndex}");
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
        {
            Debug.Log($"現在の投票者={players[currentPlayerIndex].playerName}");
        }
        Debug.Log($"========================");
        
        // 自分以外のプレイヤーのツイートを表示（匿名化のため作成者名非表示）
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log($"プレイヤー{i}: {players[i].playerName}, ツイート: '{players[i].tweetText}'");
            
            if (i != currentPlayerIndex)
            {
                Debug.Log($"  → ツイート生成します");
                CreateTweetItem(players[i], i);
            }
            else
            {
                Debug.Log($"  → スキップ (自分自身)");
            }
        }
    }
    
    /// <summary>
    /// ツイート項目生成
    /// 投票選択肢として表示するツイートUIを作成
    /// </summary>
    /// <param name="player">ツイート作成者のデータ</param>
    /// <param name="playerIndex">プレイヤーのインデックス（投票処理に使用）</param>
    private void CreateTweetItem(PlayerData player, int playerIndex)
    {
        GameObject tweetItem;
        
        // プレハブがあればそれを使用、なければシンプルなUIを生成
        if (tweetItemPrefab != null)
        {
            tweetItem = Instantiate(tweetItemPrefab, tweetListContainer);
        }
        else
        {
            // プレハブがない場合はシンプルなボタンを作成
            tweetItem = new GameObject($"TweetItem_{player.playerName}");
            tweetItem.transform.SetParent(tweetListContainer, false);
            
            var rectTransform = tweetItem.AddComponent<RectTransform>();
            // アンカーを伸縮させて親の幅に合わせる
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = new Vector2(0, 120); // 幅は親に合わせる、高さは120
            
            var button = tweetItem.AddComponent<Button>();
            var image = tweetItem.AddComponent<Image>();
            image.color = new Color(0.9f, 0.9f, 0.9f);
            
            // LayoutElementを追加して高さを固定
            var layoutElement = tweetItem.AddComponent<LayoutElement>();
            layoutElement.minHeight = 120;
            layoutElement.preferredHeight = 120;
            
            // テキスト追加
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tweetItem.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 15);
            textRect.offsetMax = new Vector2(-20, -15);
            
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = player.tweetText; // ツイート本文のみ表示（匿名化）
        }
        
        // ボタンのクリックイベント設定
        var btn = tweetItem.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnTweetSelected(playerIndex, tweetItem));
        }
        
        // テキスト設定(プレハブ使用時)
        var txtComponent = tweetItem.GetComponentInChildren<Text>();
        if (txtComponent != null)
        {
            txtComponent.text = player.tweetText;
        }
        
        Debug.Log($"ツイートアイテム作成完了: {player.playerName}");
    }
    
    /// <summary>
    /// ツイート選択時の処理
    /// 選択状態の視覚化と投票ボタンの有効化
    /// </summary>
    /// <param name="playerIndex">選択されたプレイヤーのインデックス</param>
    /// <param name="tweetItem">選択されたツイートUI</param>
    private void OnTweetSelected(int playerIndex, GameObject tweetItem)
    {
        Debug.Log($"ツイート選択: {playerIndex}");
        
        // 前回の選択を解除
        if (selectedTweetItem != null)
        {
            var prevImage = selectedTweetItem.GetComponent<Image>();
            if (prevImage != null)
            {
                prevImage.color = new Color(0.9f, 0.9f, 0.9f);
            }
        }
        
        // 新しい選択を設定
        selectedPlayerIndex = playerIndex;
        selectedTweetItem = tweetItem;
        
        // 選択状態を視覚的に表示（水色ハイライト）
        var image = tweetItem.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.7f, 0.9f, 1f); // 水色
        }
        
        // 投票ボタンを有効化
        if (voteButton != null)
        {
            voteButton.interactable = true;
            
            // 投票ボタンのリスナーを設定
            voteButton.onClick.RemoveAllListeners();
            voteButton.onClick.AddListener(OnVoteButtonClicked);
        }
    }
    
    /// <summary>
    /// 投票ボタンクリック時の処理
    /// GameManagerに投票を送信し、次のプレイヤーに遷移
    /// </summary>
    private void OnVoteButtonClicked()
    {
        if (selectedPlayerIndex < 0)
        {
            Debug.LogWarning("ツイートが選択されていません");
            return;
        }
        
        Debug.Log($"投票実行: プレイヤーインデックス {selectedPlayerIndex}");
        
        // GameManagerに投票を送信
        if (gameManager != null)
        {
            gameManager.SubmitVote(selectedPlayerIndex);
        }
        
        // 選択状態をリセット
        selectedPlayerIndex = -1;
        selectedTweetItem = null;
        
        // 投票ボタンを無効化
        if (voteButton != null)
        {
            voteButton.interactable = false;
        }
    }

    /// <summary>
    /// 結果UI表示
    /// ランキング生成とリスタート/終了ボタンの設定
    /// </summary>
    void ShowResultUI()
    {
        // すべてのUIを非表示にしてから結果UIを表示
        HideAllPanels();
        if (resultUIPanel != null)
        {
            resultUIPanel.SetActive(true);
            Debug.Log("結果UI表示");
            
            // ランキングを表示
            DisplayRanking();
            
            // ボタンのリスナー設定
            SetupResultButtons();
        }
        else
        {
            Debug.LogWarning("ResultUIPanel が見つかりません");
        }
    }
    
    /// <summary>
    /// 結果画面ボタン設定
    /// リスタートと終了ボタンのイベント登録
    /// </summary>
    private void SetupResultButtons()
    {
        // リスタートボタン
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        
        // 終了ボタン
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }
    
    /// <summary>
    /// リスタートボタンクリック時の処理
    /// タイトルシーンへの遷移またはゲームリセット
    /// </summary>
    private void OnRestartButtonClicked()
    {
        Debug.Log("リスタートボタンがクリックされました");
        
        // タイトルシーンが別にある場合はタイトルに戻る
        if (hasSeperateTitle)
        {
            SceneManager.LoadScene(titleSceneName);
        }
        else
        {
            // UIをクリア（重要！前回のデータが残らないようにする）
            ClearAllUIData();
            
            // ゲームをリセット（プレイヤー、山札、UI状態の初期化）
            if (gameManager != null)
            {
                gameManager.ResetGame();
            }
            
            // セットアップフェーズに戻る
            ChangePhase(GamePhase.Setup);
        }
    }
    
    /// <summary>
    /// 全UIデータクリア
    /// リスタート時に前回のデータが残らないよう全UI要素を削除
    /// </summary>
    private void ClearAllUIData()
    {
        Debug.Log("=== UIデータをクリア ===");
        
        // 投票フェーズの選択状態をリセット
        selectedPlayerIndex = -1;
        selectedTweetItem = null;
        
        // 投票ボタンを無効化してリスナークリア
        if (voteButton != null)
        {
            voteButton.interactable = false;
            voteButton.onClick.RemoveAllListeners();
        }
        
        // 投票開始ボタンのリスナーをクリア
        if (votingStartButton != null)
        {
            votingStartButton.onClick.RemoveAllListeners();
        }
        
        // ツイート作成開始ボタンのリスナーをクリア
        if (tweetingStartButton != null)
        {
            tweetingStartButton.onClick.RemoveAllListeners();
        }
        
        // 投票候補ツイートリストをクリア（動的生成されたUI削除）
        if (tweetListContainer != null)
        {
            foreach (Transform child in tweetListContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        // ランキングリストをクリア（動的生成されたUI削除）
        if (resultRankingContainer != null)
        {
            foreach (Transform child in resultRankingContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        // PostPanelのツイートテキストをクリア
        if (gameManager != null && gameManager.postPanel != null)
        {
            var postPanelText = gameManager.postPanel.GetComponentInChildren<Text>();
            if (postPanelText != null)
            {
                postPanelText.text = "what's happeninig?\n";
            }
        }
        
        Debug.Log("UIデータのクリア完了");
    }
    
    /// <summary>
    /// 終了ボタンクリック時の処理
    /// アプリケーション終了（エディタではプレイモード終了）
    /// </summary>
    private void OnQuitButtonClicked()
    {
        Debug.Log("終了ボタンがクリックされました");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// ランキング表示
    /// プレイヤーをスコア順にソートしてランキングUIを生成
    /// </summary>
    private void DisplayRanking()
    {
        if (gameManager == null || resultRankingContainer == null)
        {
            Debug.LogWarning("GameManager または ResultRankingContainer が null です");
            return;
        }
        
        // 既存のランキング項目をクリア
        foreach (Transform child in resultRankingContainer)
        {
            Destroy(child.gameObject);
        }
        
        // VerticalLayoutGroupを追加(なければ)
        var layoutGroup = resultRankingContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = resultRankingContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 15;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        }
        
        // ContentSizeFitterを追加(なければ)
        var contentSizeFitter = resultRankingContainer.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = resultRankingContainer.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        // プレイヤーをスコア順にソート（降順 = 高得点が上位）
        var players = gameManager.GetAllPlayers();
        var sortedPlayers = new System.Collections.Generic.List<PlayerData>(players);
        sortedPlayers.Sort((a, b) => b.score.CompareTo(a.score)); // 降順ソート
        
        Debug.Log($"=== ランキング表示 ===");
        
        // ランキング項目を作成
        int rank = 1;
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var player = sortedPlayers[i];
            
            // 同率の場合は同じ順位を表示
            if (i > 0 && sortedPlayers[i - 1].score == player.score)
            {
                // 順位は変わらない（同率順位）
            }
            else
            {
                rank = i + 1;
            }
            
            CreateRankingItem(rank, player);
            Debug.Log($"{rank}位: {player.playerName} - スコア: {player.score} - ツイート: {player.tweetText}");
        }
        
        Debug.Log($"===================");
    }
    
    /// <summary>
    /// ランキング項目生成
    /// 順位に応じた背景色とメダル表示
    /// </summary>
    /// <param name="rank">順位</param>
    /// <param name="player">プレイヤーデータ</param>
    private void CreateRankingItem(int rank, PlayerData player)
    {
        GameObject rankingItem;
        
        // プレハブがあればそれを使用、なければシンプルなUIを生成
        if (rankingItemPrefab != null)
        {
            rankingItem = Instantiate(rankingItemPrefab, resultRankingContainer);
        }
        else
        {
            // プレハブがない場合はシンプルなパネルを作成
            rankingItem = new GameObject($"RankingItem_{rank}");
            rankingItem.transform.SetParent(resultRankingContainer, false);
            
            var rectTransform = rankingItem.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = new Vector2(0, 100);
            
            var image = rankingItem.AddComponent<Image>();
            // 順位に応じた背景色（1位:金、2位:銀、3位:銅、それ以外:白）
            if (rank == 1)
                image.color = new Color(1f, 0.84f, 0f, 0.3f); // 金色
            else if (rank == 2)
                image.color = new Color(0.75f, 0.75f, 0.75f, 0.3f); // 銀色
            else if (rank == 3)
                image.color = new Color(0.8f, 0.5f, 0.2f, 0.3f); // 銅色
            else
                image.color = new Color(0.95f, 0.95f, 0.95f, 0.5f); // 白
            
            // LayoutElementを追加
            var layoutElement = rankingItem.AddComponent<LayoutElement>();
            layoutElement.minHeight = 100;
            layoutElement.preferredHeight = 100;
            
            // テキスト追加
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(rankingItem.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 10);
            textRect.offsetMax = new Vector2(-15, -10);
            
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            
            // ランキング形式で表示: メダル絵文字+順位+得票数+作成者+ツイート
            string rankText = rank == 1 ? "🏆 1位" : rank == 2 ? "🥈 2位" : rank == 3 ? "🥉 3位" : $"{rank}位";
            text.text = $"{rankText}  ({player.score}票)\n{player.playerName}: {player.tweetText}";
        }
        
        // テキスト設定(プレハブ使用時)
        var txtComponent = rankingItem.GetComponentInChildren<Text>();
        if (txtComponent != null)
        {
            string rankText = rank == 1 ? "🏆 1位" : rank == 2 ? "🥈 2位" : rank == 3 ? "🥉 3位" : $"{rank}位";
            txtComponent.text = $"{rankText}  ({player.score}票)\n{player.playerName}: {player.tweetText}";
        }
    }

    /// <summary>
    /// 全パネル非表示
    /// フェーズ切り替え時に使用
    /// </summary>
    private void HideAllPanels()
    {
        if (setupUIPanel != null) setupUIPanel.SetActive(false);
        if (tweetingStartPanel != null) tweetingStartPanel.SetActive(false);
        if (tweetingUIPanel != null) tweetingUIPanel.SetActive(false);
        if (votingStartPanel != null) votingStartPanel.SetActive(false);
        if (votingUIPanel != null) votingUIPanel.SetActive(false);
        if (resultUIPanel != null) resultUIPanel.SetActive(false);
    }

    // ========== フェーズ遷移通知メソッド ==========

    /// <summary>
    /// ツイート作成フェーズ完了通知
    /// GameManagerから呼ばれ、投票フェーズへ遷移
    /// </summary>
    public void OnTweetingPhaseComplete()
    {
        Debug.Log("→ ツイート作成フェーズ完了");
        ChangePhase(GamePhase.Voting);
    }

    /// <summary>
    /// 投票フェーズ完了通知
    /// GameManagerから呼ばれ、結果フェーズへ遷移
    /// </summary>
    public void OnVotingPhaseComplete()
    {
        Debug.Log("→ 投票フェーズ完了");
        ChangePhase(GamePhase.Result);
    }

    /// <summary>
    /// 結果フェーズ完了通知
    /// ゲーム終了時の処理（現在は未使用）
    /// </summary>
    public void OnResultPhaseComplete()
    {
        Debug.Log("→ 結果フェーズ完了");
        Debug.Log("ゲーム終了");
    }
}
