using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Building Data")]
public class BuildingData : ScriptableObject
{
    public string m_code;

    public BuildingType.TYPE m_buildingType;
    public string m_name;
    [TextArea] public string m_description;

    public Sprite m_icon;

    [Header("Initial Settings")]
    [Range(0, 999)]
    public int m_initialAmount = 0;
    public bool m_isUnlocked = true; // 건물 언락 상태

    public List<ResourceAmount> m_requireResourceList = new List<ResourceAmount>();
    public List<ResourceAmount> m_productionList = new List<ResourceAmount>();

    /// <summary>
    /// m_productionList에서 특정 ResourceType이 있는지 확인
    /// </summary>
    /// <param name="resourceType">확인할 ResourceType</param>
    /// <returns>해당 ResourceType이 있으면 true, 없으면 false</returns>
    public bool HasProductionResource(ResourceType.TYPE resourceType)
    {
        return m_productionList.Any(resource => resource.m_type == resourceType);
    }

    /// <summary>
    /// m_productionList에서 특정 ResourceType의 ResourceAmount를 가져옴
    /// </summary>
    /// <param name="resourceType">찾을 ResourceType</param>
    /// <returns>해당 ResourceType의 ResourceAmount, 없으면 null</returns>
    public ResourceAmount GetProductionResource(ResourceType.TYPE resourceType)
    {
        return m_productionList.FirstOrDefault(resource => resource.m_type == resourceType);
    }
}
