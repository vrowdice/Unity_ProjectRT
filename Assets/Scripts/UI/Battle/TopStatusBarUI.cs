using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TopStatusBarUI : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private TMP_Text allyCountText;
    [SerializeField] private Image enemySideIcon;
    [SerializeField] private Image allySideIcon;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button settingsButton;

    [Header("�ɼ�")]
    [Tooltip("���� ���� ����")]
    [SerializeField] private float refreshInterval = 0.25f;

    private BattleSystemManager bsm;
    private Coroutine loop;

    private void Awake()
    {
        var cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // ��� Graphic�� raycastTarget ��Ȱ�� �� ��ư ������ �ٽ� Ȱ��
        var allGraphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in allGraphics) g.raycastTarget = false;
        EnableButtonRaycasts(helpButton, true);
        EnableButtonRaycasts(settingsButton, true);
    }

    private void OnEnable()
    {
        bsm = BattleSystemManager.Instance;
        if (bsm != null) bsm.UnitsChanged += UpdateCountsNow;

        UpdateCountsNow();

        if (loop == null && refreshInterval > 0f)
            loop = StartCoroutine(RefreshLoop());
    }

    private void OnDisable()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
        if (bsm != null) bsm.UnitsChanged -= UpdateCountsNow;
    }

    private IEnumerator RefreshLoop()
    {
        var wait = new WaitForSeconds(refreshInterval);
        while (true)
        {
            UpdateCountsNow();
            yield return wait;
        }
    }

    private void UpdateCountsNow()
    {
        if (!enemyCountText || !allyCountText)
            return;

        if (bsm == null)
        {
            enemyCountText.text = "--";
            allyCountText.text = "--";
            return;
        }

        int enemy = CountAlive(bsm.EnemyUnits);
        int ally = CountAlive(bsm.AllyUnits);

        enemyCountText.text = enemy.ToString();
        allyCountText.text = ally.ToString();
    }

    private static int CountAlive(List<UnitBase> list)
    {
        if (list == null || list.Count == 0) return 0;
        int c = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var u = list[i];
            if (u && u.gameObject.activeSelf && !u.IsDead) c++;
        }
        return c;
    }

    private static void EnableButtonRaycasts(Button btn, bool on)
    {
        if (!btn) return;
        var g = btn.GetComponentsInChildren<Graphic>(true);
        foreach (var gg in g) gg.raycastTarget = on;
        // ��ư ��ü ��ȣ�ۿ��� ����
        var cg = btn.GetComponentInParent<CanvasGroup>();
        if (cg) { /* ��Ʈ CanvasGroup�� �̹� ���ȣ�ۿ�. ��ư ��ü�� Ŭ�� ����(�׷��ȸ� ���) */ }
    }
}
