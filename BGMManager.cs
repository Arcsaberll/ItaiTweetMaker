using UnityEngine;

/// <summary>
/// BGM管理クラス
/// 【役割】ゲーム全体のBGM再生を管理するシングルトン
/// 【機能】
/// - タイトル、セットアップ、ツイート作成、投票、結果の各フェーズごとにBGMを切り替え
/// - シーン遷移時もBGMを継続（DontDestroyOnLoad）
/// - フェードイン/アウト機能
/// 【使用方法】
/// 1. 空のGameObjectに本スクリプトをアタッチ
/// 2. Inspectorで各フェーズのBGM（AudioClip）をアサイン
/// 3. 各シーンで自動的に適切なBGMが再生される
/// </summary>
public class BGMManager : MonoBehaviour
{
    // シングルトンインスタンス（ゲーム全体で1つのみ存在）
    public static BGMManager Instance { get; private set; }

    [Header("BGM設定")]
    [SerializeField] private AudioSource audioSource;      // BGM再生用AudioSource
    [SerializeField] private AudioClip titleBGM;           // タイトル画面BGM
    [SerializeField] private AudioClip setupBGM;           // プレイヤー設定画面BGM
    [SerializeField] private AudioClip tweetingBGM;        // ツイート作成フェーズBGM
    [SerializeField] private AudioClip votingBGM;          // 投票フェーズBGM
    [SerializeField] private AudioClip resultBGM;          // 結果画面BGM
    
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.5f;       // BGM音量（0～1）
    
    [SerializeField] private float fadeSpeed = 1f;         // フェードイン/アウトの速度

    private AudioClip currentBGM;                          // 現在再生中のBGM
    private bool isFading = false;                         // フェード処理中フラグ
    private float targetVolume;                            // フェード先の音量

    /// <summary>
    /// 初期化処理
    /// シングルトンパターンを実装し、シーン遷移で破棄されないようにする
    /// </summary>
    private void Awake()
    {
        // シングルトンパターン: 既にインスタンスがあれば自身を破棄
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移で破棄されない
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSourceコンポーネントがなければ自動追加
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // BGM用の設定
        audioSource.loop = true;           // ループ再生
        audioSource.volume = bgmVolume;    // 初期音量設定
    }

    private void Update()
    {
        // フェード処理
        if (isFading)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
            
            if (Mathf.Approximately(audioSource.volume, targetVolume))
            {
                isFading = false;
                
                // フェードアウト完了時に停止
                if (targetVolume == 0f)
                {
                    audioSource.Stop();
                }
            }
        }
    }

    /// <summary>
    /// タイトル画面のBGMを再生
    /// </summary>
    public void PlayTitleBGM()
    {
        PlayBGM(titleBGM);
        Debug.Log("BGM: タイトル画面");
    }

    /// <summary>
    /// セットアップ画面のBGMを再生
    /// </summary>
    public void PlaySetupBGM()
    {
        PlayBGM(setupBGM);
        Debug.Log("BGM: セットアップ画面");
    }

    /// <summary>
    /// ツイート作成フェーズのBGMを再生
    /// </summary>
    public void PlayTweetingBGM()
    {
        PlayBGM(tweetingBGM);
        Debug.Log("BGM: ツイート作成フェーズ");
    }

    /// <summary>
    /// 投票フェーズのBGMを再生
    /// </summary>
    public void PlayVotingBGM()
    {
        PlayBGM(votingBGM);
        Debug.Log("BGM: 投票フェーズ");
    }

    /// <summary>
    /// 結果画面のBGMを再生
    /// </summary>
    public void PlayResultBGM()
    {
        PlayBGM(resultBGM);
        Debug.Log("BGM: 結果画面");
    }

    /// <summary>
    /// BGMを再生（既に同じBGMが再生中なら何もしない）
    /// </summary>
    private void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("BGMが設定されていません");
            return;
        }

        // 既に同じBGMが再生中なら何もしない
        if (currentBGM == clip && audioSource.isPlaying)
        {
            return;
        }

        currentBGM = clip;
        audioSource.clip = clip;
        audioSource.loop = true; // ループ再生を確実に設定
        audioSource.volume = bgmVolume;
        audioSource.Play();
        
        isFading = false;
    }

    /// <summary>
    /// BGMをフェードアウトして停止
    /// </summary>
    public void StopBGM()
    {
        if (audioSource.isPlaying)
        {
            isFading = true;
            targetVolume = 0f;
        }
    }

    /// <summary>
    /// BGMの音量を変更
    /// </summary>
    public void SetVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (!isFading)
        {
            audioSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// BGMを一時停止
    /// </summary>
    public void PauseBGM()
    {
        audioSource.Pause();
    }

    /// <summary>
    /// BGMを再開
    /// </summary>
    public void ResumeBGM()
    {
        audioSource.UnPause();
    }
}
