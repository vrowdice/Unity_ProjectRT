using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverightBut : MonoBehaviour
{
    private BattleManager battleManager;
    private string enemybattleBeforeUITag = "EBBUI";
    private GameObject battleBeforeUI;
    [SerializeField] private GameObject battleBeforeUIFeb;

    void Start()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
    }

    public void MoveRightCamera()
    {
        if (battleManager != null)
        {
            battleManager.isAttackField = true;
            battleBeforeUI = GameObject.FindWithTag(enemybattleBeforeUITag);
            Canvas canvas = FindObjectOfType<Canvas>();
            battleManager.battleBeforeUI = Instantiate(battleBeforeUIFeb, canvas.transform);
            Destroy(battleBeforeUI);
        }
    }
}
