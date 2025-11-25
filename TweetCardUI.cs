using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TweetCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TweetCard cardData;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); // UIの最上層に移動
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
    }
}
