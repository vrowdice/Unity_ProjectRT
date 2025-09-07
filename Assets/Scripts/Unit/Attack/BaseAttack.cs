using System;
using System.Collections;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("���� ������")]
    public float attackRange;

    // ����
    public bool IsAttacking { get; protected set; } // Active ����(���� ���� ����) ��
    public bool IsBusy { get; protected set; }      // Active+Recovery ��ü ����Ŭ ��
    public float ActiveSec { get; protected set; }
    public float RecoverySec { get; protected set; }

    // ���� ����
    protected UnitBase owner;
    protected GameObject target;
    private Coroutine attackCoroutine;

    // ���� ����/�ߺ� ������
    protected UnitBase attackerCache;
    private bool _cycleEndedRaised = false;

    // �̺�Ʈ
    public event Action<UnitBase> OnAttackCycleStarted;
    public event Action<UnitBase> OnAttackCycleEnded;
    public event Action<UnitBase, int> OnHit;

    protected void RaiseAttackCycleStarted(UnitBase attacker)
    {
        attackerCache = attacker;   // �����ߴ� ���
        _cycleEndedRaised = false;      // �� ����Ŭ ����
        OnAttackCycleStarted?.Invoke(attacker);
    }
    protected void RaiseAttackCycleEnded(UnitBase attacker)
    {
        if (_cycleEndedRaised) return;  // �ߺ� ����
        _cycleEndedRaised = true;
        OnAttackCycleEnded?.Invoke(attacker);
        attackerCache = null;
    }
    protected void RaiseHit(UnitBase attacker, int hitIndex) => OnHit?.Invoke(attacker, hitIndex);

    public void SetTempo(float activeSec, float recoverySec)
    {
        ActiveSec = Mathf.Max(0.0f, activeSec);
        RecoverySec = Mathf.Max(0.0f, recoverySec);
    }

    public virtual bool TryStartSkill(UnitBase attacker, GameObject targetEnemy) => false;

    protected bool CanRunCoroutine() => isActiveAndEnabled && gameObject.activeInHierarchy;
    protected Coroutine StartRoutineSafe(IEnumerator routine) => CanRunCoroutine() ? StartCoroutine(routine) : null;

    public void StartAttack(UnitBase attacker, GameObject targetEnemy)
    {
        if (IsBusy) return;
        if (attacker == null || targetEnemy == null || !targetEnemy.activeSelf) return;
        if (!CanRunCoroutine()) return; // ��Ȱ���� �� ���� ����

        owner = attacker;
        target = targetEnemy;

        attackerCache = attacker;
        _cycleEndedRaised = false;

        attackCoroutine = StartRoutineSafe(AttackCycle());
    }

    public void StopAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = null;

        // ����Ŭ ���� ���� �ߴ� �ÿ��� ���� �̺�Ʈ�� �� �� ����
        if (!_cycleEndedRaised && attackerCache != null)
            RaiseAttackCycleEnded(attackerCache);

        IsAttacking = false;
        IsBusy = false;

        owner = null;
        target = null;
        attackerCache = null;
    }

    private IEnumerator AttackCycle()
    {
        if (!CanRunCoroutine()) yield break;

        IsBusy = true;
        IsAttacking = true;

        if (owner != null && target != null)
        {
            owner.NotifyAttackActiveBegin();
            UnitImpactEmitter.Emit(owner.gameObject, ImpactEventType.BasicAttackStart, owner, target, 0.0f, null);
            owner.NotifyBasicAttackStart(target);
        }

        // ���� ���� ����(�Ļ� Ŭ���� ����)
        yield return PerformAttackRoutine(owner, target);

        // Active ����
        IsAttacking = false;
        owner?.NotifyAttackActiveEnd();

        // �ĵ�
        if (RecoverySec > 0.0f)
            yield return new WaitForSeconds(RecoverySec);

        // �Ļ��� ���� �̺�Ʈ�� �����ص� ���⼭ 1ȸ ����
        if (!_cycleEndedRaised && owner != null)
            RaiseAttackCycleEnded(owner);

        IsBusy = false;
        attackCoroutine = null;

        // ���� ����(�޸�/Ǯ�� ����)
        owner = null;
        target = null;
    }

    protected abstract IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target);

    protected void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (!IsTargetAlive(target)) return;

        var unit = target.GetComponent<UnitBase>();
        if (unit == null) return;
        if (unit.Team == attacker.Team) return; // �Ʊ� ��ȣ

        unit.TakeDamage(rawDamage);
        attacker.OnBasicAttackLanded();
        attacker.NotifyBasicAttackHit(target, rawDamage);

        UnitImpactEmitter.Emit(attacker.gameObject, ImpactEventType.BasicAttackHit, attacker, target, rawDamage, null);
        UnitImpactEmitter.Emit(target, ImpactEventType.Damaged, attacker, target, rawDamage, null);
    }

    protected float GetRange(UnitBase attacker)
        => (attackRange > 0.0f) ? attackRange : attacker.AttackRange;

    protected bool IsTargetAlive(GameObject go)
    {
        if (go == null || !go.activeSelf) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    protected bool InEffectiveRange(Transform self, Transform tgt, float mul = 1.0f)
    {
        if (self == null || tgt == null) return false;
        float r = GetRange(owner) * mul;
        return Vector3.Distance(self.position, tgt.position) <= r;
    }

    private void OnDisable() => StopAttack();
}
