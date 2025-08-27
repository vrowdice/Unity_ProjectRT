using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class BattleResultUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text resultLabel; 
    [SerializeField] private Canvas hostCanvas;     

    public Action OnContinue;
    public Action OnRetry;

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!resultLabel) resultLabel = GetComponentInChildren<TMP_Text>(true);
        if (!hostCanvas) hostCanvas = GetComponentInParent<Canvas>(true);
    }

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!resultLabel) resultLabel = GetComponentInChildren<TMP_Text>(true);
        if (!hostCanvas) hostCanvas = GetComponentInParent<Canvas>(true);
    }

    public void Show(BattleOutcome outcome)
    {
        if (!resultLabel)
        {
            resultLabel = GetComponentInChildren<TMP_Text>(true);
            if (!resultLabel)
            {
                Debug.LogError("[BattleResultUI] 결과 라벨(TMP_Text)을 찾을 수 없습니다.");
            }
        }

        if (resultLabel)
        {
            bool win = outcome.victory;
            resultLabel.text = win ? "WIN" : "LOSE";
            resultLabel.color = win ? new Color(0.2f, 0.9f, 0.3f, 1.0f) : new Color(0.95f, 0.25f, 0.25f, 1.0f);
            resultLabel.gameObject.SetActive(true);
            resultLabel.ForceMeshUpdate(true, true);
        }

        if (hostCanvas)
        {
            if (hostCanvas.renderMode == RenderMode.ScreenSpaceOverlay ||
                hostCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (hostCanvas.sortingOrder < 5000) hostCanvas.sortingOrder = 5000;
            }
        }

        if (canvasGroup)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
    }

    public void Hide()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }
}
