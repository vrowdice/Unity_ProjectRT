using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text resultLabel;     
/*    [SerializeField] private Button continueButton;    
    [SerializeField] private Button retryButton;       */

    public Action OnContinue;
    public Action OnRetry;

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
/*        if (continueButton) continueButton.onClick.AddListener(() => OnContinue?.Invoke());
        if (retryButton) retryButton.onClick.AddListener(() => OnRetry?.Invoke());*/
    }

    public void Show(BattleOutcome outcome)
    {
        if (resultLabel)
        {
            bool win = outcome.victory;
            resultLabel.text = win ? "WIN" : "LOSE";
            resultLabel.color = win ? new Color(0.2f, 0.9f, 0.3f) : new Color(0.95f, 0.25f, 0.25f);
        }

        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
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
