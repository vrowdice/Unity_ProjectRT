using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockBuildingEffect", menuName = "Event Effects/Building/Unlock")]
public class UnlockBuildingEffect : EffectBase
{
    [Header("Specific Building")]
    [Tooltip("Enter building code to specify a particular building to unlock.")]
    public string m_specificBuildingCode = "";

    private GameDataManager m_gameDataManager = null;

    public override void Activate(GameDataManager argDataManager, string argEventName)
    {
        m_gameDataManager = argDataManager;

        if (!string.IsNullOrEmpty(m_specificBuildingCode))
        {
            argDataManager.UnlockSpecificBuilding(m_specificBuildingCode, 1);
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        // 건물 언락은 영구적이므로 제거할 필요 없음
    }

    public override string GetEffectInfo()
    {
        string baseInfo = base.GetEffectInfo();
        
        if (!IsActive)
        {
            return baseInfo;
        }
        
        string buildingInfo = $"{m_specificBuildingCode} 건물 언락";
        
        return $"{baseInfo}\n{buildingInfo}";
    }
} 