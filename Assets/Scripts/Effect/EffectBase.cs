using UnityEngine;

public abstract class EffectBase : ScriptableObject
{
    public int m_duration = -1; // -1이면 무한 지속
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
        if (m_duration < 0) return false; // 무한 지속이면 무시

        m_remainingTurns--;

        if (m_remainingTurns <= 0)
        {
            Deactivate(dataManager);
            return true; // 제거 필요
        }

        return false;
    }
}

