using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class BattleLoadingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        StartCoroutine(ShowLoadingThenHide());
    }

    private IEnumerator ShowLoadingThenHide()
    {
        loadingPanel?.SetActive(true);
        if (progressBar) progressBar.fillAmount = 0.0f;
        if (loadingText) loadingText.text = "Checking...";

        yield return new WaitForSeconds(0.2f);

        if (loadingText) loadingText.text = "Finish!";
        yield return new WaitForSeconds(0.2f);
        loadingPanel?.SetActive(false);
    }
}
