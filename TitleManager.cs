using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// タイトルシーン管理クラス
/// 【役割】タイトル画面の制御とシーン遷移
/// 【主要機能】
/// - マウスクリックでゲームシーンへ遷移
/// - BGMManagerの自動生成とタイトルBGM再生
/// 【使用方法】
/// 1. タイトルシーンに空のGameObjectを作成
/// 2. このスクリプトをアタッチ
/// 3. InspectorでnextSceneName（遷移先シーン名）を設定
/// 4. BGMManagerPrefabは任意（なければ自動生成される）
/// </summary>
public class Scene : MonoBehaviour
{
    [Header("シーン設定")]
    [SerializeField] private string nextSceneName = "Setting";          // 遷移先シーン名
    
    [Header("BGM設定")]
    [SerializeField] private GameObject bgmManagerPrefab;               // BGMManagerのプレハブ（オプション）
    
    private BGMManager bgmManager;                                      // BGM管理クラスへの参照

    /// <summary>
    /// 初期化処理
    /// BGMManagerの取得/生成とタイトルBGM再生
    /// </summary>
    private void Start()
    {
        // BGMManagerを取得または生成
        bgmManager = FindObjectOfType<BGMManager>();
        
        if (bgmManager == null)
        {
            Debug.LogWarning("BGMManager が見つかりません。");
            
            // プレハブが設定されていれば生成
            if (bgmManagerPrefab != null)
            {
                GameObject bgmObj = Instantiate(bgmManagerPrefab);
                bgmManager = bgmObj.GetComponent<BGMManager>();
                Debug.Log("BGMManager をプレハブから生成しました");
            }
            else
            {
                // プレハブがない場合は空のGameObjectに追加
                GameObject bgmObj = new GameObject("BGMManager");
                bgmManager = bgmObj.AddComponent<BGMManager>();
                Debug.Log("BGMManager を新規作成しました");
            }
        }
        
        // タイトルBGMを再生
        if (bgmManager != null)
        {
            bgmManager.PlayTitleBGM();
        }
        else
        {
            Debug.LogError("BGMManager の取得/生成に失敗しました");
        }
    }

    /// <summary>
    /// 更新処理
    /// マウス左クリック検出でシーン遷移
    /// </summary>
    void Update()
    {
        // 左クリックでゲームシーンへ遷移
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("シーン移動");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

}
