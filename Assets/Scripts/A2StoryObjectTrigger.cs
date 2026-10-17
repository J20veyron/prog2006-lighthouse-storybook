using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class A2StoryObjectTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool collectsMemory;
    [TextArea(2, 4)] public string feedbackText;
    public AudioClip feedbackClip;

    private Image image;
    private RectTransform rect;
    private Vector3 restingScale;
    private Color restingColor;
    private bool isAnimating;

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        if (rect != null) restingScale = rect.localScale;
        if (image != null) restingColor = image.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return;
        StartCoroutine(Pulse());

        var page = FindObjectOfType<A2StoryPageController>();
        if (page == null) return;

        if (collectsMemory)
            page.CollectMemory(feedbackText, feedbackClip);
        else
            page.ShowObjectFeedback(feedbackText, feedbackClip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rect != null && !isAnimating)
            rect.localScale = restingScale * 1.06f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rect != null && !isAnimating)
            rect.localScale = restingScale;
    }

    private IEnumerator Pulse()
    {
        isAnimating = true;
        var elapsed = 0f;
        const float duration = 0.28f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var amount = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            if (rect != null) rect.localScale = restingScale * (1f + 0.18f * amount);
            if (image != null) image.color = Color.Lerp(restingColor, new Color32(255, 219, 112, 255), 0.45f * amount);
            yield return null;
        }
        if (rect != null) rect.localScale = restingScale;
        if (image != null) image.color = restingColor;
        isAnimating = false;
    }
}
