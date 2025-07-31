using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveleftBut : MonoBehaviour
{
    private BattleManager battleManager;

    void Start()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
    }

    public void MoveLeftCamera()
    {
        if (battleManager != null)
        {
            battleManager.isAttackField = false;
        }
    }
}
