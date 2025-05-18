using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFactionData", menuName = "Faction Data")]
public class FactionData : ScriptableObject
{
    public FactionType m_factionType; // 팩션 유형 (enum)
    public string m_name;
    [TextArea]
    public string m_description;     // 설명
    public Sprite m_illustration;
    public Sprite m_icon;            // 아이콘
    public Color m_factionColor;     // 고유 색상

    [TextArea]
    public string m_traitDescription; // 고유 특성 설명
    public List<ResearchData> m_uniqueResearch;
}