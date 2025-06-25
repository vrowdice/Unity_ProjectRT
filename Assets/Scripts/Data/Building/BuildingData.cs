using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Building Data")]
public class BuildingData : ScriptableObject
{
    [HideInInspector] public string m_code;

    public BuildingType.TYPE m_buildingType;
    public string m_name;
    [TextArea] public string m_description;

    public Sprite m_icon;

    public List<ResourceAmount> m_requireResourceList = new List<ResourceAmount>();
    public List<ResourceAmount> m_productionList = new List<ResourceAmount>();
}
