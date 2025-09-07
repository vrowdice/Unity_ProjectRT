using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMultiAttackReusable : BaseSkill
{
    [Header("스킬 수치")]
    [Min(0)] public float SkillDamageCoefficient = 1.0f;
    [Min(1)] public int SkillAttackCount = 2;  
    [Min(0)] public int ManaCostOverride = -1;  

    public enum TargetCollectMode { AroundPrimary, AroundCaster }

    [Header("타겟 수집 옵션")]
    public TargetCollectMode collectMode = TargetCollectMode.AroundPrimary;

    [Tooltip("검색 반경 = (기준 사거리) * multiplier. 기준은 caster.AttackRange")]
    public float extraTargetSearchRadiusMultiplier = 1.0f;

    public enum DistanceAnchor { Primary, Caster }
    [Tooltip("가까운 순 정렬 기준")]
    public DistanceAnchor sortBy = DistanceAnchor.Primary;

    [Tooltip("히트 사이에 프레임 텀(0이면 같은 프레임에 연속 적용)")]
    [Min(0)] public int hitFrameGap = 0;

    [Header("히트 부가효과")]
    public StatusType onHitApplyStatus = StatusType.None;
    public float onHitStatusDuration = 5f;

    [Tooltip("피격자가 특정 상태일 때 처치되면 발생시킬 커스텀 이벤트명(빈 문자열이면 미사용)")]
    public string onKillTriggerEventName = "";
    [Tooltip("위 이벤트의 필수 상태(None이면 조건 없음)")]
    public StatusType onKillRequiredStatus = StatusType.MarkHunted;

    private readonly List<UnitBase> _targets = new List<UnitBase>(8);

    private static readonly Collider2D[] s_buf = new Collider2D[64];

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject primaryTargetGO)
    {
        // 코스트 덮어쓰기
        if (ManaCostOverride >= 0) this.manaCost = ManaCostOverride;

        // 기본 유효성
        if (!ValidateCaster(caster)) yield break;

        // 중심/정렬 기준 결정
        UnitBase primary = null;
        if (collectMode == TargetCollectMode.AroundPrimary)
        {
            if (!ValidateTargetGO(primaryTargetGO)) yield break;
            primary = primaryTargetGO.GetComponent<UnitBase>();
            if (!IsAttackableEnemy_Team(caster, primary)) yield break;
        }

        Vector3 center =
            (collectMode == TargetCollectMode.AroundPrimary && primary != null)
            ? primary.transform.position : caster.transform.position;

        Vector3 sortPivot =
            (sortBy == DistanceAnchor.Primary && primary != null)
            ? primary.transform.position : caster.transform.position;

        float radius = Mathf.Max(0.01f, caster.AttackRange) *
                       Mathf.Max(0.01f, extraTargetSearchRadiusMultiplier);

        // 타겟 수집 
        _targets.Clear();
        CollectTargets_NoAlloc(caster, primary, center, sortPivot, radius, SkillAttackCount, _targets);

        if (_targets.Count == 0) yield break;

        // 피해 계산
        float damage = Mathf.Max(0f, SkillDamageCoefficient) * caster.AttackPower;

        // 히트 루프
        for (int i = 0; i < _targets.Count; i++)
        {
            var victim = _targets[i];
            if (!IsAttackableEnemy_Team(caster, victim)) continue;

            bool hadReqStatus = (onKillRequiredStatus == StatusType.None) ||
                                HasStatus(victim, onKillRequiredStatus);

            victim.TakeDamage(damage);

            UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit,
                                   caster, victim.gameObject, damage, skillName);

            if (onHitApplyStatus != StatusType.None)
                TryApplyStatus(victim, onHitApplyStatus, onHitStatusDuration, caster);

            if (victim.IsDead && !string.IsNullOrEmpty(onKillTriggerEventName) && hadReqStatus)
                TriggerCustomEvent(caster, victim, onKillTriggerEventName);

            for (int f = 0; f < hitFrameGap; f++)
                yield return null;
        }
    }

    private void CollectTargets_NoAlloc(
        UnitBase caster, UnitBase primary, Vector3 center, Vector3 sortPivot,
        float radius, int maxCount, List<UnitBase> outList)
    {
        var tgt = caster.GetComponent<UnitTargetingController>();
        if (tgt != null)
        {
            if (collectMode == TargetCollectMode.AroundPrimary && primary)
                outList.Add(primary);

            int want = Mathf.Max(0, maxCount - outList.Count);
            if (want > 0)
            {
                tgt.AcquireTargetsNonAlloc(outList, center, radius, want, preferCurrentFirst: false);
            }

            if (primary)
                DeduplicateKeepFirst(outList, primary);

            if (sortPivot != center) 
                outList.Sort((a, b) => DistSqr(sortPivot, a).CompareTo(DistSqr(sortPivot, b)));

            if (outList.Count > maxCount)
                outList.RemoveRange(maxCount, outList.Count - maxCount);

            return;
        }

        int n = Physics2D.OverlapCircleNonAlloc(center, radius, s_buf, TeamLayers.GetEnemyMask(caster.Team));
        if (collectMode == TargetCollectMode.AroundPrimary && primary)
            outList.Add(primary);

        for (int i = 0; i < n; i++)
        {
            var col = s_buf[i];
            if (!col) continue;
            var ub = col.GetComponent<UnitBase>();
            if (!IsAttackableEnemy_Team(caster, ub)) continue;
            if (collectMode == TargetCollectMode.AroundPrimary && ub == primary) continue;

            outList.Add(ub);
        }

        outList.Sort((a, b) => DistSqr(sortPivot, a).CompareTo(DistSqr(sortPivot, b)));

        if (outList.Count > maxCount)
            outList.RemoveRange(maxCount, outList.Count - maxCount);
    }

    private static void DeduplicateKeepFirst(List<UnitBase> list, UnitBase key)
    {
        bool seen = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == key)
            {
                if (!seen) { seen = true; continue; }
                list.RemoveAt(i); i--;
            }
        }
    }

    private static float DistSqr(Vector3 pivot, UnitBase ub)
    {
        if (!ub) return float.MaxValue;
        var d = ub.transform.position - pivot;
        return d.sqrMagnitude;
    }

    private bool ValidateCaster(UnitBase c) => c != null && !c.IsDead;

    private bool ValidateTargetGO(GameObject go)
    {
        if (!go || !go.activeSelf) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub && !ub.IsDead;
    }

    private bool IsAttackableEnemy_Team(UnitBase caster, UnitBase target)
    {
        if (!caster || !target) return false;
        if (target.IsDead) return false;
        return caster.Team != target.Team;
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
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.CustomEvent,
                               caster, victim.gameObject, 0f, eventName);

        var broker = FindObjectOfType<BattleEventBroker>();
        if (broker) broker.Raise(eventName, caster, victim);
        else Debug.Log($"[SkillMultiAttack] Event '{eventName}' fired by {caster?.UnitName} -> {victim?.UnitName}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var caster = GetComponent<UnitBase>();
        if (!caster) return;

        float radius = Mathf.Max(0.01f, caster.AttackRange) *
                       Mathf.Max(0.01f, extraTargetSearchRadiusMultiplier);

        Gizmos.color = (collectMode == TargetCollectMode.AroundPrimary) ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
