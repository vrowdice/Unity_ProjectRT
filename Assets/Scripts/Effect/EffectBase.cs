using UnityEngine;

public abstract class EffectBase : ScriptableObject
{
    public int m_duration = -1; // -1�̸� ���� ����
    private int m_remainingTurns;

    public void Init(int duration)
    {
        m_duration = duration;
        m_remainingTurns = duration;
    }

    public abstract void Activate(GameDataManager dataManager);
    public abstract void Deactivate(GameDataManager dataManager);

    public bool Tick(GameDataManager dataManager)
    {
        if (m_duration < 0) return false; // ���� �����̸� ����

        m_remainingTurns--;

        if (m_remainingTurns <= 0)
        {
            Deactivate(dataManager);
            return true; // ���� �ʿ�
        }

        return false;
    }
}

