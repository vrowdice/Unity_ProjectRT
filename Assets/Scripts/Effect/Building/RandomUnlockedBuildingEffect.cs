using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomUnlockedBuildingEffect", menuName = "Event Effects/Building/Random Add Unlocked")]
public class RandomUnlockedBuildingEffect : EffectBase
{
    [Header("Building Settings")]
    public int m_addCount = 1;

    private GameDataManager m_gameDataManager = null;

    public override void Activate(GameDataManager argDataManager, string argEventName)
    {
        m_gameDataManager = argDataManager;
        argDataManager.RandomUnlockedBuilding(m_addCount);
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        // 건물 추가는 영구적이므로 제거할 필요 없음
    }

    public override string GetEffectInfo()
    {
        string baseInfo = base.GetEffectInfo();
        
        if (!IsActive)
        {
            return baseInfo;
        }
        
        string buildingInfo = $"언락된 건물 중 랜덤 {m_addCount}개 추가";
        
        return $"{baseInfo}\n{buildingInfo}";
    }
} 