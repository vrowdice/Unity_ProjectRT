using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFactionData", menuName = "Faction Data")]
public class FactionData : ScriptableObject
{
    public FactionType m_factionType; // �Ѽ� ���� (enum)
    public string m_name;
    [TextArea]
    public string m_description;     // ����
    public Sprite m_illustration;
    public Sprite m_icon;            // ������
    public Color m_factionColor;     // ���� ����

    [TextArea]
    public string m_traitDescription; // ���� Ư�� ����
    public List<ResearchData> m_uniqueResearch;
}