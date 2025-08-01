using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveleftBut : MonoBehaviour
{
    private BattleManager battleManager;
    private string battleBeforeUITag = "BBUI";
    private GameObject battleBeforeUI;
   [SerializeField] private GameObject enemyBattleBeforeUIFeb;

    void Start()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
    }

    public void MoveLeftCamera()
    {
        if (battleManager != null)
        {
            battleManager.isAttackField = false;
            battleBeforeUI = GameObject.FindWithTag(battleBeforeUITag);
            Canvas canvas = FindObjectOfType<Canvas>();
            battleManager.enemyBattleBeforeUI = Instantiate(enemyBattleBeforeUIFeb, canvas.transform);
            Destroy(battleBeforeUI);
        }
    }
}
