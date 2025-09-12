using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResearchData", menuName = "Research Data")]
public class FactionResearchData : ScriptableObject
{
    public string m_code;
    public string m_name;
    public string m_description;

    public int m_cost;
    public float m_duration; // 연구 완료 시간

    public List<FactionResearchData> m_prerequisites; // 선행 연구 ID 목록
    public List<FactionResearchData> m_unlocks;       // 해금되는 연구 ID 목록

    public List<EffectBase> m_effects; //연구 완료 시 활성화되는 이펙트

    public Sprite m_icon;                // 연구 아이콘
    public bool m_isFirstLocked = false;

    public FactionType.TYPE m_factionType; //추가: 해당 연구의 진영 타입

    [Header("Faction Requirements")]
    public int m_requiredFactionLike = 0; // 필요한 우호도 (기본값 0 = 조건 없음)

    /// <summary>
    /// 팩션 우호도 조건 확인 (일반 연구는 항상 true, 팩션별 연구는 우호도 체크)
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    /// <returns>조건 만족 여부</returns>
    public bool CheckFactionLikeRequirement(GameDataManager dataManager)
    {
        // 일반 연구(None)이거나 조건 없음
        if (m_factionType == FactionType.TYPE.None || m_requiredFactionLike <= 0)
            return true;
            
        var factionEntry = dataManager.GetFactionEntry(m_factionType);
        return factionEntry != null && factionEntry.m_state.m_like >= m_requiredFactionLike;
    }

    /// <summary>
    /// 연구 완료 시 모든 이펙트 활성화
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    /// <param name="researchEntry">연구 엔트리 (이펙트 추적용)</param>
    public void ActivateAllEffect(GameDataManager dataManager, FactionResearchEntry researchEntry)
    {
        foreach(EffectBase effect in m_effects)
        {
            if (effect.ActivateEffect(dataManager, m_name))
            {
                // 활성화 성공 시 ResearchEntry에 추가
                researchEntry.AddActiveEffect(m_code, effect);
            }
        }
    }

    /// <summary>
    /// 연구 효과 비활성화 (필요시 사용)
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    /// <param name="researchEntry">연구 엔트리 (이펙트 추적용)</param>
    public void DeactivateAllEffect(GameDataManager dataManager, FactionResearchEntry researchEntry)
    {
        foreach (EffectBase effect in m_effects)
        {
            if (effect.DeactivateEffect(dataManager))
            {
                researchEntry.RemoveActiveEffect(m_code, effect);
            }
        }
    }
}
