using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AllyArmyData", menuName = "AllyArmyData")]
public class AllyArmyData : ScriptableObject
{
    public List<UnitStatBase> units;
}
