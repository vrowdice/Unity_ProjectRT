using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResearchData", menuName = "Research Data")]
public class ResearchData : ScriptableObject
{
    public string m_code;
    public string m_name;
    public string m_description;

    public int m_cost;
    public float m_duration; // 연구 완료 시간

    public List<ResearchData> m_prerequisites; // 선행 연구 ID 목록
    public List<ResearchData> m_unlocks;       // 해금되는 연구 ID 목록

    public List<EffectBase> m_effects; //연구 완료 시 활성화되는 이펙트

    public Sprite m_icon;                // 연구 아이콘
    public bool m_isFirstLocked = false;

    public FactionType.TYPE m_factionType; //추가: 해당 연구의 진영 타입

    /// <summary>
    /// 연구 완료 시 모든 이펙트 활성화
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void ActivateAllEffect(GameDataManager dataManager)
    {
        foreach(EffectBase item in m_effects)
        {
            item.Activate(dataManager);
        }
    }

    /// <summary>
    /// 연구 효과 비활성화 (필요시 사용)
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void DeactivateAllEffect(GameDataManager dataManager)
    {
        foreach (EffectBase item in m_effects)
        {
            item.Deactivate(dataManager);
        }
    }
}
