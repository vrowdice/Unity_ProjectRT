using UnityEngine;

// 패시브 스킬 
public abstract class PassiveSkillHandler : MonoBehaviour
{
    public abstract void ApplyEffect(UnitBase unit);

    public virtual void RemoveEffect(UnitBase unit)
    {
        // 제거 가능한 패시브는 자식 클래스에서 구현
    }
}