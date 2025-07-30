using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLoadingManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("�ε� UI")]
    public GameObject loadingPanel;
    private Image loadingBar;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitializeBattleScene());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator InitializeBattleScene()
    {
        loadingPanel.SetActive(true);
        Image loadingBar = loadingPanel.transform.Find("LoadingBar").GetComponent<Image>(); ;

        // �ε� �ܰ� ����Ʈ ����
        List<IEnumerator> initSteps = new()
    {
        LoadArmyData(),
        CombatTypeCheck(),
        LoadEvent(),
        LoadMapSetting(),
        SetupCamera()
    };

        int stepCount = initSteps.Count;
        for (int i = 0; i < stepCount; i++)
        {
            yield return StartCoroutine(initSteps[i]);

            float progress = (i + 1f) / stepCount;
            UpdateLoadingBar(progress);
        }

        yield return new WaitForSeconds(0.5f);
        loadingPanel.SetActive(false);
    }


    private void UpdateLoadingBar(float value)
    {
        if (loadingBar != null)
        {
            loadingBar.fillAmount = Mathf.Clamp01(value);
        }
    }

    private IEnumerator LoadArmyData()
    {
        Debug.Log("���� �ε� ��...");
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator CombatTypeCheck()
    {
        Debug.Log("����Ÿ�� Ȯ�� ��...");
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LoadEvent()
    {
        Debug.Log("����Ÿ�� Ȯ�� ��...");
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LoadMapSetting()
    {
        Debug.Log("�� ���� ��...");
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SetupCamera()
    {
        Debug.Log("ī�޶�/UI ���� ��...");
        yield return new WaitForSeconds(0.5f);
    }
}
