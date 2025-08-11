using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddBuildingEffect", menuName = "Event Effects/Building/Add")]
public class AddBuildingEffect : EffectBase
{
    [Header("Building Settings")]
    public int m_addCount = 1;
    
    [Header("Specific Building (Optional)")]
    [Tooltip("Enter building code to specify a particular building. Leave empty to add random building.")]
    public string m_specificBuildingCode = "";

    private GameDataManager m_gameDataManager = null;
    private List<string> m_addedBuildings = new List<string>(); // 추가된 건물들을 추적

    public override void Activate(GameDataManager argDataManager, string argEventName)
    {
        m_gameDataManager = argDataManager;
        m_addedBuildings.Clear(); // 새로운 활성화 시 추적 초기화

        // Check if specific building is specified
        if (!string.IsNullOrEmpty(m_specificBuildingCode))
        {
            // Add specific building
            if (argDataManager.AddSpecificBuilding(m_specificBuildingCode, m_addCount))
            {
                for (int i = 0; i < m_addCount; i++)
                {
                    m_addedBuildings.Add(m_specificBuildingCode);
                }
            }
        }
        else
        {
            // Add random building
            List<string> addedBuildings = argDataManager.RandomBuilding(m_addCount);
            if (addedBuildings != null)
            {
                m_addedBuildings.AddRange(addedBuildings);
            }
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        // 추가된 건물들을 제거 (필요한 경우)
        // 주의: 건물 제거 로직이 GameDataManager에 구현되어 있어야 함
        if (m_addedBuildings.Count > 0)
        {
            Debug.Log($"AddBuildingEffect: {m_addedBuildings.Count} buildings were added and should be removed if needed.");
            // TODO: 건물 제거 로직 구현 필요
            // foreach (string buildingCode in m_addedBuildings)
            // {
            //     argDataManager.RemoveBuilding(buildingCode);
            // }
        }
        
        m_addedBuildings.Clear();
    }

    /// <summary>
    /// 이펙트 정보를 사용자에게 표시할 수 있는 문자열 반환 (오버라이드)
    /// </summary>
    /// <returns>이펙트 정보 문자열</returns>
    public override string GetEffectInfo()
    {
        string baseInfo = base.GetEffectInfo();
        
        if (!IsActive)
        {
            return baseInfo;
        }
        
        string buildingInfo = string.IsNullOrEmpty(m_specificBuildingCode) 
            ? $"랜덤 건물 {m_addCount}개" 
            : $"{m_specificBuildingCode} 건물 {m_addCount}개";
        
        string addedInfo = m_addedBuildings.Count > 0 
            ? $"\n추가된 건물: {string.Join(", ", m_addedBuildings)}"
            : "";
        
        return $"{baseInfo}\n{buildingInfo}{addedInfo}";
    }
}
