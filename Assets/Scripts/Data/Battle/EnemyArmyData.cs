using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyArmyData", menuName = "EnemyArmyData")]
public class EnemyArmyData : ScriptableObject
{
    public List<UnitData> units;
}