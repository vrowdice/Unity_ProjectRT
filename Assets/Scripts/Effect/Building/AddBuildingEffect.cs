using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddBuildingEffect", menuName = "Event Effects/Building/Add")]
public class AddBuildingEffect : EffectBase
{
    [Header("Building Settings")]
    public int m_addCount = 1;
    
    [Header("Specific Building")]
    [Tooltip("Enter building code to specify a particular building to add.")]
    public string m_specificBuildingCode = "";

    private GameDataManager m_gameDataManager = null;

    public override void Activate(GameDataManager argDataManager, string argEventName)
    {
        m_gameDataManager = argDataManager;

        if (!string.IsNullOrEmpty(m_specificBuildingCode))
        {
            argDataManager.AddSpecificBuilding(m_specificBuildingCode, m_addCount);
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        // 건물 추가는 영구적이므로 제거할 필요 없음
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
        
        string buildingInfo = $"{m_specificBuildingCode} 건물 {m_addCount}개";
        
        return $"{baseInfo}\n{buildingInfo}";
    }
}
