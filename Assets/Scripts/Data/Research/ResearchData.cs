using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResearchData", menuName = "Research Data")]
public class ResearchData : ScriptableObject
{
    public string m_code;
    public string m_name;
    [TextArea] public string m_description;

    public int m_cost;
    public float m_duration; // ���� �ҿ� �ð�

    public List<string> m_prerequisites; // ���� ���� ID ���
    public List<string> m_unlocks;       // �رݵǴ� ��� ID ���

    public Sprite m_icon;                // ���� ������
    public bool m_isFirstLocked = false;

    public FactionType m_factionType; //�߰�: �ش� ������ ���� �Ѽ�
}
