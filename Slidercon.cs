using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI要素に必要
public class Slidercon : MonoBehaviour
{
    [SerializeField] private Slider m_Slider; // 音量調整用のスライダー
    [SerializeField] private AudioSource m_AudioSource; // 対象のAudioSource

    void Awake()
    {
        if (m_Slider == null || m_AudioSource == null)
        {
            Debug.LogError("SliderまたはAudioSourceがアタッチされていません！");
            return;
        }

        // スライダーの初期値をAudioSourceの音量に設定
        m_Slider.value = m_AudioSource.volume;

        // スライダーの値変更時に音量を更新
        m_Slider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDisable()
    {
        // イベントリスナーを解除（メモリリーク防止）
        m_Slider.onValueChanged.RemoveAllListeners();
    }

    // スライダーの値から音量を設定
    private void SetVolume(float sliderValue)
    {
        if (m_AudioSource != null)
        {
            m_AudioSource.volume = sliderValue;
        }
    }
}

