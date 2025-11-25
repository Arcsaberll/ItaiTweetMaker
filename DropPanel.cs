using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class DropPanel : MonoBehaviour, IDropHandler
{
    [Header("パネルに表示する Text (通常の UI Text)")]
    public Text panelText;

    // TextMeshPro を使う場合はこちらをセットする（片方だけセットしてください）
#if TMP_PRESENT
    [Header("または TextMeshProUGUI (TMP)")]
    public TextMeshProUGUI panelTextTMP;
#endif

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null) return;

        // ドロップされた GameObject（pointerDrag）を取得
        GameObject draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        // まず DraggableText コンポーネントを探す
        DraggableText draggable = draggedObj.GetComponent<DraggableText>();

        string addText = null;

        if (draggable != null)
        {
            // DraggableText に textValue がある場合はそれを使う
            addText = draggable.textValue;
        }
        else
        {
            // もし DraggableText が無ければ、Text または TMP のテキストを直接取る（フォールバック）
            var uiText = draggedObj.GetComponent<Text>();
            if (uiText != null) addText = uiText.text;

#if TMP_PRESENT
            var tmp = draggedObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null) addText = tmp.text;
#endif
        }

        if (string.IsNullOrEmpty(addText)) return;

        // パネルのテキストに追加する（どちらのタイプがセットされているか確認）
#if TMP_PRESENT
        if (panelTextTMP != null)
        {
            panelTextTMP.text += addText;
            return;
        }
#endif
        if (panelText != null)
        {
            panelText.text += addText;
        }
    }
}