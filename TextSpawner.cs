using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; 

public class TextSpawner : MonoBehaviour
{
    [Header("生成する UI プレハブ (DraggableText)")]
    public GameObject draggableTextPrefab;

    [Header("カードを並べるパネル（SpawnArea）")]
    public Transform spawnArea;

    [Header("GameManager参照")]
    public GameManager gameManager;

    void Start()
    {
        // GameManagerが未設定の場合は自動で検索
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    // 現在のプレイヤーの手札を表示
    public void SpawnCurrentPlayerHand()
    {
        Debug.Log("=== SpawnCurrentPlayerHand 開始 ===");
        
        if (gameManager == null)
        {
            Debug.LogError("TextSpawner: GameManagerが null です！");
            return;
        }

        PlayerData currentPlayer = gameManager.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Debug.LogError("TextSpawner: 現在のプレイヤーが null です！");
            return;
        }

        Debug.Log($"TextSpawner: 現在のプレイヤー = {currentPlayer.playerName}, 手札枚数 = {currentPlayer.hand.Count}");
        SpawnHandCards(currentPlayer.hand);
    }

    // リセット時に使用（現在のプレイヤーの手札を再表示）
    public void SpawnFromSavedHand()
    {
        SpawnCurrentPlayerHand();
    }

    // 指定された手札カードを表示
    private void SpawnHandCards(List<TweetCard> handCards)
    {
        Debug.Log($"=== SpawnHandCards 開始: handCards = {(handCards == null ? "null" : handCards.Count + "枚")} ===");
        
        // spawnArea の確認
        if (spawnArea == null)
        {
            Debug.LogError("TextSpawner: spawnArea が null です！Inspectorで設定してください。");
            return;
        }
        Debug.Log($"spawnArea: {spawnArea.name}, active: {spawnArea.gameObject.activeInHierarchy}, 親: {(spawnArea.parent != null ? spawnArea.parent.name : "なし")}");
        
        // 古い手札 UI を削除
        int childCount = spawnArea.childCount;
        Debug.Log($"spawnArea の既存の子オブジェクト数: {childCount}");
        foreach (Transform child in spawnArea)
            Destroy(child.gameObject);

        if (handCards == null || handCards.Count == 0)
        {
            Debug.LogWarning("TextSpawner: 表示する手札がありません (handCards が null または空)");
            return;
        }

        if (draggableTextPrefab == null)
        {
            Debug.LogError("TextSpawner: draggableTextPrefab が null です！Inspectorで設定してください。");
            return;
        }
        Debug.Log($"draggableTextPrefab: {draggableTextPrefab.name}");

        // 手札を表示
        int successCount = 0;
        foreach (TweetCard card in handCards)
        {
            if (card == null)
            {
                Debug.LogWarning("TextSpawner: null のカードをスキップしました");
                continue;
            }

            GameObject obj = Instantiate(draggableTextPrefab, spawnArea);
            obj.name = $"Card_{card.cardText}";
            
            // 明示的にオブジェクトを有効化
            obj.SetActive(true);
            
            Debug.Log($"  生成したオブジェクト: {obj.name}, 親: {obj.transform.parent.name}, active: {obj.activeInHierarchy}");

            Text t = obj.GetComponent<Text>();
            DraggableText dt = obj.GetComponent<DraggableText>();

            if (t != null)
            {
                t.text = card.cardText;
                t.enabled = true; // Text コンポーネントを明示的に有効化
                Debug.Log($"    Text コンポーネント設定: {t.text}, enabled: {t.enabled}, fontSize: {t.fontSize}, color: {t.color}");
            }
            else
            {
                Debug.LogWarning($"    Text コンポーネントが見つかりません");
            }
            
            if (dt != null)
            {
                dt.textValue = card.cardText;
                dt.enabled = true; // DraggableText を明示的に有効化
                Debug.Log($"    DraggableText 設定完了");
            }
            else
            {
                Debug.LogWarning($"    DraggableText コンポーネントが見つかりません");
            }
            
            // RectTransform の設定を確認
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"    RectTransform: width={rectTransform.rect.width}, height={rectTransform.rect.height}, scale={rectTransform.localScale}");
            }
            
            successCount++;
            Debug.Log($"  カード生成: {card.cardText}");
        }

        // Canvas の更新を強制
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"TextSpawner: {successCount}枚の手札を表示しました");
        Debug.Log($"spawnArea の最終的な子オブジェクト数: {spawnArea.childCount}");
    }
}
