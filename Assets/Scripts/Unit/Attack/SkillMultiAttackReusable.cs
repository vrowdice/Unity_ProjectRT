using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMultiAttackReusable : BaseSkill
{
    [Header("스킬 수치 (기획 컬럼 매핑)")]
    [Min(0)] public float SkillDamageCoefficient = 1.0f;   
    [Min(1)] public int SkillAttackCount = 2;         
    [Min(0)] public int ManaCostOverride = -1;    

    public enum TargetCollectMode
    {
        AroundPrimary, 
        AroundCaster    
    }

    [Header("타겟 수집 옵션")]
    public TargetCollectMode collectMode = TargetCollectMode.AroundPrimary;

    [Tooltip("추가 타겟을 찾을 반경 = 기준거리(공격사거리) * multiplier")]
    public float extraTargetSearchRadiusMultiplier = 1.0f;

    public enum DistanceAnchor { Primary, Caster }
    [Tooltip("가까운 순 정렬 기준")]
    public DistanceAnchor sortBy = DistanceAnchor.Primary;

    //0이면 같은 프레임에 연속 적용
    [Tooltip("히트 순서 간 프레임 텀")]
    [Min(0)] public int hitFrameGap = 0;

    [Header("히트 효과 옵션")]
    [Tooltip("히트 시 부여할 상태. None이면 미사용")]
    public StatusType onHitApplyStatus = StatusType.None;

    [Tooltip("상태 지속시간(초). 0 이하는 즉시 만료")]
    public float onHitStatusDuration = 5f;

    [Tooltip("피격자가 특정 상태일 때 본 스킬로 처치되면 발동할 커스텀 이벤트명")]
    public string onKillTriggerEventName = "";

    [Tooltip("onKillTriggerEventName을 발동시키기 위한 '필수 상태' (None이면 조건 없이 발동)")]
    public StatusType onKillRequiredStatus = StatusType.MarkHunted;

    // 내부 버퍼
    private readonly List<UnitBase> _targets = new List<UnitBase>(8);

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject primaryTargetGO)
    {
        if (ManaCostOverride >= 0) this.manaCost = ManaCostOverride;

        if (!ValidateCaster(caster)) yield break;

        // 1차 타겟 유효성 확인 (AroundPrimary일 때는 반드시 필요)
        UnitBase primary = null;
        if (collectMode == TargetCollectMode.AroundPrimary)
        {
            if (!ValidateTargetGO(primaryTargetGO)) yield break;
            primary = primaryTargetGO.GetComponent<UnitBase>();
            if (!IsAttackableEnemy(caster, primary)) yield break;
        }

        // 타겟 수집
        _targets.Clear();
        CollectTargets(caster, primary, _targets, SkillAttackCount);

        if (_targets.Count == 0) yield break;

        // 피해 계산
        float damage = Mathf.Max(0f, SkillDamageCoefficient) * caster.AttackPower;

        // 히트 루프
        for (int i = 0; i < _targets.Count; i++)
        {
            var victim = _targets[i];
            if (!IsAttackableEnemy(caster, victim)) continue;

            bool hadReqStatus = (onKillRequiredStatus == StatusType.None) ? true : HasStatus(victim, onKillRequiredStatus);

            // 데미지 적용
            victim.TakeDamage(damage);

            // 히트 이펙트/로그
            UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit,
                                   caster, victim.gameObject, damage, skillName);

            // 히트 시 상태 부여
            if (onHitApplyStatus != StatusType.None)
                TryApplyStatus(victim, onHitApplyStatus, onHitStatusDuration, caster);

            // 처치 + 조건 상태 충족 시 커스텀 이벤트 트리거
            if (victim.IsDead && !string.IsNullOrEmpty(onKillTriggerEventName) && hadReqStatus)
            {
                TriggerCustomEvent(caster, victim, onKillTriggerEventName);
            }

            // 히트 간 텀
            for (int f = 0; f < hitFrameGap; f++)
                yield return null;
        }

        yield break;
    }

    // 타겟 수집 로직
    private void CollectTargets(UnitBase caster, UnitBase primary, List<UnitBase> buffer, int maxCount)
    {
        // 검색 중심/정렬 기준 결정
        Vector3 center = (collectMode == TargetCollectMode.AroundPrimary && primary != null)
            ? primary.transform.position
            : caster.transform.position;

        Vector3 sortPivot = (sortBy == DistanceAnchor.Primary && primary != null)
            ? primary.transform.position
            : caster.transform.position;

        // 반경
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

        // 거리 기준 정렬
        buffer.Sort((a, b) =>
            Vector3.Distance(sortPivot, a.transform.position).CompareTo(
            Vector3.Distance(sortPivot, b.transform.position)));

        // 최대 수만 유지
        if (buffer.Count > maxCount) buffer.RemoveRange(maxCount, buffer.Count - maxCount);
    }

    // ====== 유틸 ======
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
        return caster.Faction != target.Faction; // 진영 비교
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
        // 공통 이펙트/로그
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.CustomEvent,
                               caster, victim.gameObject, 0f, eventName);

        // 중앙화된 시스템에 위임(실제 버프/보너스 효과는 여기서 구현)
        var broker = FindObjectOfType<BattleEventBroker>();
        if (broker) broker.Raise(eventName, caster, victim);
        else Debug.Log($"[SkillMultiAttack] Event '{eventName}' fired by {caster.UnitName}");
    }
}
