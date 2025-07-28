using UnityEngine;

// �нú� ��ų 
public abstract class PassiveSkillHandler : MonoBehaviour
{
    public abstract void ApplyEffect(UnitBase unit);

    public virtual void RemoveEffect(UnitBase unit)
    {
        // ���� ������ �нú�� �ڽ� Ŭ�������� ����
    }
}