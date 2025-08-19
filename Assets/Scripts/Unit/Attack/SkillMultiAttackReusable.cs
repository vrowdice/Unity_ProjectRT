using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMultiAttackReusable : BaseSkill
{
    [Header("��ų ��ġ (��ȹ �÷� ����)")]
    [Min(0)] public float SkillDamageCoefficient = 1.0f;   
    [Min(1)] public int SkillAttackCount = 2;         
    [Min(0)] public int ManaCostOverride = -1;    

    public enum TargetCollectMode
    {
        AroundPrimary, 
        AroundCaster    
    }

    [Header("Ÿ�� ���� �ɼ�")]
    public TargetCollectMode collectMode = TargetCollectMode.AroundPrimary;

    [Tooltip("�߰� Ÿ���� ã�� �ݰ� = ���ذŸ�(���ݻ�Ÿ�) * multiplier")]
    public float extraTargetSearchRadiusMultiplier = 1.0f;

    public enum DistanceAnchor { Primary, Caster }
    [Tooltip("����� �� ���� ����")]
    public DistanceAnchor sortBy = DistanceAnchor.Primary;

    //0�̸� ���� �����ӿ� ���� ����
    [Tooltip("��Ʈ ���� �� ������ ��")]
    [Min(0)] public int hitFrameGap = 0;

    [Header("��Ʈ ȿ�� �ɼ�")]
    [Tooltip("��Ʈ �� �ο��� ����. None�̸� �̻��")]
    public StatusType onHitApplyStatus = StatusType.None;

    [Tooltip("���� ���ӽð�(��). 0 ���ϴ� ��� ����")]
    public float onHitStatusDuration = 5f;

    [Tooltip("�ǰ��ڰ� Ư�� ������ �� �� ��ų�� óġ�Ǹ� �ߵ��� Ŀ���� �̺�Ʈ��")]
    public string onKillTriggerEventName = "";

    [Tooltip("onKillTriggerEventName�� �ߵ���Ű�� ���� '�ʼ� ����' (None�̸� ���� ���� �ߵ�)")]
    public StatusType onKillRequiredStatus = StatusType.MarkHunted;

    // ���� ����
    private readonly List<UnitBase> _targets = new List<UnitBase>(8);

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject primaryTargetGO)
    {
        if (ManaCostOverride >= 0) this.manaCost = ManaCostOverride;

        if (!ValidateCaster(caster)) yield break;

        // 1�� Ÿ�� ��ȿ�� Ȯ�� (AroundPrimary�� ���� �ݵ�� �ʿ�)
        UnitBase primary = null;
        if (collectMode == TargetCollectMode.AroundPrimary)
        {
            if (!ValidateTargetGO(primaryTargetGO)) yield break;
            primary = primaryTargetGO.GetComponent<UnitBase>();
            if (!IsAttackableEnemy(caster, primary)) yield break;
        }

        // Ÿ�� ����
        _targets.Clear();
        CollectTargets(caster, primary, _targets, SkillAttackCount);

        if (_targets.Count == 0) yield break;

        // ���� ���
        float damage = Mathf.Max(0f, SkillDamageCoefficient) * caster.AttackPower;

        // ��Ʈ ����
        for (int i = 0; i < _targets.Count; i++)
        {
            var victim = _targets[i];
            if (!IsAttackableEnemy(caster, victim)) continue;

            bool hadReqStatus = (onKillRequiredStatus == StatusType.None) ? true : HasStatus(victim, onKillRequiredStatus);

            // ������ ����
            victim.TakeDamage(damage);

            // ��Ʈ ����Ʈ/�α�
            UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit,
                                   caster, victim.gameObject, damage, skillName);

            // ��Ʈ �� ���� �ο�
            if (onHitApplyStatus != StatusType.None)
                TryApplyStatus(victim, onHitApplyStatus, onHitStatusDuration, caster);

            // óġ + ���� ���� ���� �� Ŀ���� �̺�Ʈ Ʈ����
            if (victim.IsDead && !string.IsNullOrEmpty(onKillTriggerEventName) && hadReqStatus)
            {
                TriggerCustomEvent(caster, victim, onKillTriggerEventName);
            }

            // ��Ʈ �� ��
            for (int f = 0; f < hitFrameGap; f++)
                yield return null;
        }

        yield break;
    }

    // Ÿ�� ���� ����
    private void CollectTargets(UnitBase caster, UnitBase primary, List<UnitBase> buffer, int maxCount)
    {
        // �˻� �߽�/���� ���� ����
        Vector3 center = (collectMode == TargetCollectMode.AroundPrimary && primary != null)
            ? primary.transform.position
            : caster.transform.position;

        Vector3 sortPivot = (sortBy == DistanceAnchor.Primary && primary != null)
            ? primary.transform.position
            : caster.transform.position;

        // �ݰ�
        float radius = Mathf.Max(0.01f, caster.AttackRange) * Mathf.Max(0.01f, extraTargetSearchRadiusMultiplier);

        var all = GameObject.FindObjectsOfType<UnitBase>();

        if (collectMode == TargetCollectMode.AroundPrimary && primary != null)
            buffer.Add(primary);

        foreach (var u in all)
        {
            if (u == null || u.IsDead) continue;
            if (!IsAttackableEnemy(caster, u)) continue;

            if (collectMode == TargetCollectMode.AroundPrimary && u == primary) continue;

            float dist = Vector3.Distance(center, u.transform.position);
            if (dist <= radius) buffer.Add(u);
        }

        // �Ÿ� ���� ����
        buffer.Sort((a, b) =>
            Vector3.Distance(sortPivot, a.transform.position).CompareTo(
            Vector3.Distance(sortPivot, b.transform.position)));

        // �ִ� ���� ����
        if (buffer.Count > maxCount) buffer.RemoveRange(maxCount, buffer.Count - maxCount);
    }

    // ====== ��ƿ ======
    private bool ValidateCaster(UnitBase c) => c != null && !c.IsDead;

    private bool ValidateTargetGO(GameObject go)
    {
        if (go == null || !go.activeSelf) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    private bool IsAttackableEnemy(UnitBase caster, UnitBase target)
    {
        if (caster == null || target == null) return false;
        if (target.IsDead) return false;
        return caster.Faction != target.Faction; // ���� ��
    }

    private bool HasStatus(UnitBase unit, StatusType status)
    {
        var sc = unit.GetComponent<IStatusController>();
        return sc != null && sc.Has(status);
    }

    private void TryApplyStatus(UnitBase unit, StatusType status, float duration, UnitBase applier)
    {
        var sc = unit.GetComponent<IStatusController>();
        if (sc != null) sc.Apply(status, duration, applier);
    }

    private void TriggerCustomEvent(UnitBase caster, UnitBase victim, string eventName)
    {
        // ���� ����Ʈ/�α�
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.CustomEvent,
                               caster, victim.gameObject, 0f, eventName);

        // �߾�ȭ�� �ý��ۿ� ����(���� ����/���ʽ� ȿ���� ���⼭ ����)
        var broker = FindObjectOfType<BattleEventBroker>();
        if (broker) broker.Raise(eventName, caster, victim);
        else Debug.Log($"[SkillMultiAttack] Event '{eventName}' fired by {caster.UnitName}");
    }
}
