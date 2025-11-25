using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetupUIManager : MonoBehaviour
{
    [SerializeField] private Dropdown playerCountDropdown;
    [SerializeField] private Transform playerNameInputContainer;
    [SerializeField] private GameObject playerNameInputPrefab;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject setupUIPanel; // SetupUIPanel への参照

    private GameManager gameManager;
    private GameFlowManager gameFlowManager;
    private List<InputField> playerNameInputs = new List<InputField>();

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameFlowManager = FindObjectOfType<GameFlowManager>();
        
        // setupUIPanel が未設定の場合、SetupUIPanel という名前のオブジェクトを検索
        if (setupUIPanel == null)
        {
            setupUIPanel = GameObject.Find("SetupUIPanel");
            if (setupUIPanel == null)
            {
                // SetupUIPanel が見つからない場合のみ、このオブジェクトを使用
                // ただし、Canvas は絶対に非表示にしない
                Transform current = transform;
                while (current != null)
                {
                    if (current.GetComponent<Canvas>() != null)
                    {
                        // Canvas に到達したら、その一つ下の子を使用
                        break;
                    }
                    if (current.parent != null && current.parent.GetComponent<Canvas>() != null)
                    {
                        // 親が Canvas の場合、このオブジェクトを使用
                        setupUIPanel = current.gameObject;
                        break;
                    }
                    current = current.parent;
                }
                
                // それでも見つからない場合は、このオブジェクト自身を使用
                if (setupUIPanel == null)
                {
                    setupUIPanel = gameObject;
                }
            }
        }
    }

    private void Start()
    {
        // ドロップダウンのセットアップ
        SetupPlayerCountDropdown();

        // 初期プレイヤー数（3人）の入力フィールドを生成
        UpdatePlayerNameInputs(3);
        // GameManager に初期選択人数を反映
        if (gameManager != null)
        {
            gameManager.selectedPlayerCount = 3;
        }

        // スタートボタンのリスナー設定
        startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void SetupPlayerCountDropdown()
    {
        playerCountDropdown.ClearOptions();
        List<string> options = new List<string> { "3人", "4人", "5人", "6人" };
        playerCountDropdown.AddOptions(options);
        playerCountDropdown.value = 0; // 初期値を 3人 に設定
        playerCountDropdown.onValueChanged.AddListener(OnPlayerCountChanged);

        // ドロップダウンのテキストサイズを大きくする
        Text captionText = playerCountDropdown.captionText;
        if (captionText != null)
        {
            captionText.fontSize = 36;
            captionText.fontStyle = FontStyle.Bold;
        }

        // ドロップダウンアイテムのテキストサイズを大きくする
        foreach (var item in playerCountDropdown.options)
        {
            // オプション生成時に設定する処理は別途必要
        }
    }

    private void OnPlayerCountChanged(int index)
    {
        int playerCount = index + 3; // 0→3, 1→4, 2→5, 3→6
        UpdatePlayerNameInputs(playerCount);
        // GameManager に選択人数を反映
        if (gameManager != null)
        {
            gameManager.selectedPlayerCount = playerCount;
        }
        
        // ドロップダウンメニューのテキストサイズを更新
        EnforceDropdownTextSize();
    }

    private void EnforceDropdownTextSize()
    {
        // ドロップダウンのテンプレートを取得
        Transform template = playerCountDropdown.template;
        if (template != null)
        {
            foreach (Transform item in template.Find("Viewport/Content"))
            {
                Text itemText = item.GetComponent<Text>();
                if (itemText == null)
                {
                    itemText = item.GetComponentInChildren<Text>();
                }
                if (itemText != null)
                {
                    itemText.fontSize = 36;
                    itemText.fontStyle = FontStyle.Bold;
                }
            }
        }
    }

    private void UpdatePlayerNameInputs(int playerCount)
    {
        // 既存のインプットフィールドをクリア（逆順で削除）
        for (int i = playerNameInputContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(playerNameInputContainer.GetChild(i).gameObject);
        }
        playerNameInputs.Clear();
        
        Debug.Log($"既存のInputFieldを削除しました。残り: {playerNameInputContainer.childCount}");

        // コンテナのレイアウトグループを取得・設定
        LayoutGroup layoutGroup = playerNameInputContainer.GetComponent<LayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = playerNameInputContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        if (layoutGroup is VerticalLayoutGroup vlg)
        {
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
        }

        // 新しいインプットフィールドを生成
        for (int i = 0; i < playerCount; i++)
        {
            GameObject inputObj = Instantiate(playerNameInputPrefab, playerNameInputContainer);
            inputObj.name = $"PlayerNameInput_{i}";

            // RectTransform を設定
            RectTransform rectTransform = inputObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            // LayoutElement を設定（高さを固定）
            LayoutElement layoutElement = inputObj.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = inputObj.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = 300; // InputField全体の高さを300に設定

            // プレハブのルートに InputField がない場合は子オブジェクトから探す
            InputField inputField = inputObj.GetComponent<InputField>();
            if (inputField == null)
            {
                inputField = inputObj.GetComponentInChildren<InputField>(true);
            }

            if (inputField != null)
            {
                inputField.text = $"プレイヤー{i + 1}";
                
                // InputField自体のRectTransformサイズを設定
                RectTransform inputFieldRect = inputField.GetComponent<RectTransform>();
                if (inputFieldRect != null)
                {
                    inputFieldRect.sizeDelta = new Vector2(500, 120); // 高さを280に設定
                }
                
                // InputFieldのテキストサイズを大きくする
                if (inputField.textComponent != null)
                {
                    inputField.textComponent.fontSize = 40; // フォントサイズを40に設定
                }
                
                playerNameInputs.Add(inputField);
            }
            else
            {
                Debug.LogWarning($"InputField コンポーネントが見つかりません: {inputObj.name} (プレハブ内を検索しました)");
            }
        }

        Debug.Log($"プレイヤー入力フィールドを {playerCount} 個生成しました");
        
        // レイアウトを強制更新
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)playerNameInputContainer);
    }

    private void OnStartGameClicked()
    {
        // プレイヤー数を取得
        int playerCount = playerCountDropdown.value + 3;

        // 入力されたプレイヤー名を取得
        List<string> playerNames = new List<string>();
        Debug.Log($"=== プレイヤー名取得開始 (InputFields: {playerNameInputs.Count}個) ===");
        
        for (int i = 0; i < playerNameInputs.Count; i++)
        {
            if (playerNameInputs[i] == null)
            {
                Debug.LogWarning($"InputField[{i}] が null です");
                continue;
            }
            
            string name = playerNameInputs[i].text;
            Debug.Log($"InputField[{i}] の値: '{name}'");
            
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"プレイヤー{i + 1}";
                Debug.Log($"  → 空白のため、デフォルト名を使用: {name}");
            }
            playerNames.Add(name);
        }
        
        Debug.Log($"=== 最終的なプレイヤー名リスト ({playerNames.Count}人) ===");
        for (int i = 0; i < playerNames.Count; i++)
        {
            Debug.Log($"  プレイヤー{i + 1}: {playerNames[i]}");
        }

        // GameManager にプレイヤーを設定
        if (gameManager != null)
        {
            gameManager.InitializeGameWithPlayers(playerNames);
        }
        else
        {
            Debug.LogError("GameManager が null です！");
        }

        // セットアップUI を完全に非表示
        if (setupUIPanel != null)
        {
            setupUIPanel.SetActive(false);
            Debug.Log("SetupUIPanel を非表示にしました");
        }
        else
        {
            // フォールバック: このスクリプトがアタッチされているオブジェクトを非表示
            gameObject.SetActive(false);
        }

        // ゲーム開始
        if (gameFlowManager != null)
        {
            gameFlowManager.ChangePhase(GamePhase.Tweeting);
        }
    }
}
