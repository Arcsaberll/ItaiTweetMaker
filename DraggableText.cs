using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableText : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string textValue;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    // ドロップ可能エリアへの参照
    private RectTransform spawnArea;
    private RectTransform postPanel;
    private Text postPanelText;
    private PostPanelUI postPanelUI; // PostPanelUIへの参照を追加

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Textから自動取得
        Text txt = GetComponent<Text>();
        if (txt != null && string.IsNullOrEmpty(textValue))
            textValue = txt.text;
        
        // ドロップエリアを検索
        FindDropAreas();
    }

    void FindDropAreas()
    {
        // SpawnArea を検索
        GameObject spawnAreaObj = GameObject.Find("spawnarea");
        if (spawnAreaObj != null)
        {
            spawnArea = spawnAreaObj.GetComponent<RectTransform>();
            Debug.Log($"SpawnArea を検出: {spawnAreaObj.name}");
        }
        else
        {
            Debug.LogWarning("SpawnArea が見つかりません");
        }

        // PostPanel を検索
        GameObject postPanelObj = GameObject.Find("PostPanel");
        if (postPanelObj != null)
        {
            postPanel = postPanelObj.GetComponent<RectTransform>();
            
            // PostPanelUIコンポーネントを取得
            postPanelUI = postPanelObj.GetComponent<PostPanelUI>();
            if (postPanelUI != null)
            {
                Debug.Log("PostPanelUIコンポーネントを検出");
            }
            else
            {
                Debug.LogWarning("PostPanelにPostPanelUIコンポーネントがありません");
            }
            
            // PostPanel内の"tuer"という名前のTextを探す
            Transform tuerTransform = postPanelObj.transform.Find("tuer");
            if (tuerTransform != null)
            {
                postPanelText = tuerTransform.GetComponent<Text>();
                if (postPanelText != null)
                {
                    // テキストが下に伸びるように設定
                    postPanelText.verticalOverflow = VerticalWrapMode.Overflow;
                    postPanelText.horizontalOverflow = HorizontalWrapMode.Wrap;
                    
                    // RectTransformの設定：上端を固定して下に伸びるように
                    RectTransform textRect = tuerTransform.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        // アンカーを上部中央に設定（横幅を35%程度に）
                        textRect.anchorMin = new Vector2(0.325f, 0.8f);
                        textRect.anchorMax = new Vector2(0.675f, 0.95f);
                        // ピボットも上部中央に設定
                        textRect.pivot = new Vector2(0.5f, 0.9f);
                        // 左右のマージンを設定
                        textRect.offsetMin = new Vector2(20, textRect.offsetMin.y);
                        textRect.offsetMax = new Vector2(-20, textRect.offsetMax.y);
                    }
                    
                    // ContentSizeFitterを追加して自動的に高さを調整
                    ContentSizeFitter fitter = tuerTransform.GetComponent<ContentSizeFitter>();
                    if (fitter == null)
                    {
                        fitter = tuerTransform.gameObject.AddComponent<ContentSizeFitter>();
                    }
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    
                    // 最前面に表示されるようにHierarchyの最後に移動
                    tuerTransform.SetAsLastSibling();
                    
                    // CanvasGroupで表示を確実にする
                    CanvasGroup cg = tuerTransform.GetComponent<CanvasGroup>();
                    if (cg == null)
                    {
                        cg = tuerTransform.gameObject.AddComponent<CanvasGroup>();
                    }
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = false; // テキストなのでRaycastは不要
                    
                    Debug.Log($"PostPanel内のtuerテキストを検出: {postPanelText.text}");
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
            Debug.LogWarning("PostPanel が見つかりません");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // カードの中心位置を取得
        Vector2 cardCenter = rectTransform.position;
        Debug.Log($"カードドロップ: {textValue}, 位置: {cardCenter}");

        // PostPanel 内にあるかチェック
        if (postPanel != null && RectTransformUtility.RectangleContainsScreenPoint(postPanel, cardCenter, canvas.worldCamera))
        {
            Debug.Log($"PostPanel内にドロップ検出: {textValue}");
            // PostPanel にドロップ
            DropToPostPanel();
        }
        // SpawnArea 内にあるかチェック
        else if (spawnArea != null && RectTransformUtility.RectangleContainsScreenPoint(spawnArea, cardCenter, canvas.worldCamera))
        {
            Debug.Log($"SpawnArea内にドロップ検出: {textValue}");
            // 元の位置に戻る
            ReturnToOriginalPosition();
        }
        else
        {
            Debug.Log($"どのエリアにもドロップされませんでした: {textValue}");
            // どちらのエリアにも入っていない場合は元の位置に戻る
            ReturnToOriginalPosition();
        }
    }

    private void DropToPostPanel()
    {
        Debug.Log($"DropToPostPanel呼び出し: textValue={textValue}");
        
        // PostPanel のテキストに追加
        if (postPanelText != null)
        {
            Debug.Log($"postPanelText検出。現在のテキスト: '{postPanelText.text}'");
            
            // 初期メッセージをクリア（改行やスペースの違いを考慮）
            string currentText = postPanelText.text.Trim();
            if (currentText == "what's happeninig?" || currentText == "what's happening?")
            {
                Debug.Log("初期メッセージをクリアします");
                postPanelText.text = "";
            }
            
            // カードの文字列を連結
            postPanelText.text += textValue;
            Debug.Log($"PostPanel に追加完了: {textValue} (現在のテキスト: '{postPanelText.text}')");
            
            // PostButtonを有効化
            EnablePostButton();
        }
        else
        {
            Debug.LogError("postPanelText が null です！");
        }

        // カードを削除
        Debug.Log($"カードを削除: {textValue}");
        Destroy(gameObject);
    }
    
    private void EnablePostButton()
    {
        // PostPanelUIから直接postButtonを取得
        if (postPanelUI != null && postPanelUI.postButton != null)
        {
            postPanelUI.postButton.interactable = true;
            Debug.Log($"PostButtonを有効化しました (PostPanelUI経由, interactable={postPanelUI.postButton.interactable})");
            return;
        }
        
        // PostPanelUIが取得できていない場合はTweetingUIPanel直下から探す
        Button postButton = null;
        
        GameObject tweetingUIPanel = GameObject.Find("TweetingUIPanel");
        if (tweetingUIPanel != null)
        {
            Debug.Log("EnablePostButton: TweetingUIPanelを検出");
            
            // TweetingUIPanel配下の全ての子オブジェクトをログ出力
            Debug.Log("=== TweetingUIPanel配下の子オブジェクト一覧 ===");
            for (int i = 0; i < tweetingUIPanel.transform.childCount; i++)
            {
                Transform child = tweetingUIPanel.transform.GetChild(i);
                Debug.Log($"  子 {i}: {child.name} (active: {child.gameObject.activeSelf})");
            }
            Debug.Log("========================================");
            
            // TweetingUIPanel直下のPostbuttonを探す (Unity上の名前はPostbutton)
            Transform postButtonTransform = tweetingUIPanel.transform.Find("Postbutton");
            if (postButtonTransform != null)
            {
                postButton = postButtonTransform.GetComponent<Button>();
                Debug.Log("EnablePostButton: TweetingUIPanel直下のPostbuttonを検出");
            }
            else
            {
                Debug.LogWarning("EnablePostButton: TweetingUIPanel直下にPostbuttonが見つかりません");
            }
        }
        else
        {
            Debug.LogWarning("EnablePostButton: TweetingUIPanelが見つかりません");
        }
        
        // それでも見つからない場合はシーン全体から探す
        if (postButton == null)
        {
            GameObject postButtonObj = GameObject.Find("Postbutton");
            if (postButtonObj != null)
            {
                postButton = postButtonObj.GetComponent<Button>();
                Debug.Log("EnablePostButton: Postbuttonをシーン全体から検出");
            }
        }
        
        if (postButton != null)
        {
            postButton.interactable = true;
            Debug.Log($"PostButtonを有効化しました (interactable={postButton.interactable})");
        }
        else
        {
            Debug.LogWarning("EnablePostButton: PostButtonが見つかりません");
        }
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private void ReturnToOriginalPosition()
    {
        // 元の位置に戻る
        rectTransform.anchoredPosition = originalPosition;
        transform.SetParent(originalParent);
        Debug.Log($"カードを元の位置に戻しました: {textValue}");
    }
}