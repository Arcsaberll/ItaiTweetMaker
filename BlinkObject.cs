using UnityEngine;
using TMPro;
public class BlinkText : MonoBehaviour
{
    private TMP_Text text;
    private float time;
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }
    void Update()
    {
        time += Time.deltaTime * 3.5f; // 点滅速度調整
        Color color = text.color;
        color.a = Mathf.Sin(time) * 0.5f + 0.5f; // アルファ値を周期的に変化
        text.color = color;
    }
}