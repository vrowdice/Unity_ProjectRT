using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResearchData", menuName = "Research Data")]
public class ResearchData : ScriptableObject
{
    public string m_code;
    public string m_name;
    [TextArea] public string m_description;

    public int m_cost;
    public float m_duration; // 연구 소요 시간

    public List<string> m_prerequisites; // 선행 연구 ID 목록
    public List<string> m_unlocks;       // 해금되는 요소 ID 목록

    public Sprite m_icon;                // 연구 아이콘
    public bool m_isFirstLocked = false;

    public FactionType m_factionType; //추가: 해당 연구가 속한 팩션
}
