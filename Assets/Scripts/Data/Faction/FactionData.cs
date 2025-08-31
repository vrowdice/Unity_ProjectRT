using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFactionData", menuName = "Faction Data")]
public class FactionData : ScriptableObject
{
    public FactionType.TYPE m_factionType; // 팩션 타입 (enum)
    public string m_name;
    [TextArea]
    public string m_description;     // 설명
    public Sprite m_illustration;
    public Sprite m_icon;            // 아이콘
    public Color m_factionColor;     // 팩션 색상

    [TextArea]
    public string m_traitDescription; // 팩션 특성 설명
    
    [Header("Research Data")]
    public List<ResearchData> m_research;   // 연구 데이터 (None 팩션 = 일반 연구, 다른 팩션 = 고유 연구)

    [Header("Unit Data")]
    public List<UnitBase> m_unitData;
}