using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum DebugLinkType { Attack, Heal, Buff, Debuff, Support }

public struct LinkInfo
{
    public UnitBase target;
    public DebugLinkType type;
    public float weight; 
    public LinkInfo(UnitBase t, DebugLinkType tp, float w = 1.0f)
    { target = t; type = tp; weight = Mathf.Clamp01(w); }
}

[DefaultExecutionOrder(2000)]
public class TargetingDebugOverlay : MonoBehaviour
{
    public static TargetingDebugOverlay Instance { get; private set; }

    [Header("표시 마스터 스위치")]
    public bool overlayEnabled = true;

    [Header("표시 토글(팀 기준)")]
    public bool showAllySources = true;
    public bool showEnemySources = true;

    [Header("공통 스타일")]
    [Range(0.005f, 0.1f)] public float baseWidth = 0.03f;
    [Range(0.1f, 2.0f)] public float ttlSeconds = 0.35f;   
    [Range(1f, 5f)] public float pulseWidthBoost = 2.3f;
    [Range(0.05f, 1.0f)] public float pulseDuration = 0.15f;
    public Material lineMaterial; 

    [Header("자동 스캔/리플렉션")]
    public bool autoScanUnits = true;
    public bool tryReflection = true;
    [Range(0.02f, 0.5f)] public float reflectInterval = 0.15f;

    private class LinkVisual
    {
        public LineRenderer lr;
        public UnitBase source;
        public UnitBase target;
        public DebugLinkType type;
        public float weight;
        public float lastUpdateTime;
        public float pulseUntil;
        public float baseAlpha;
    }

    private readonly Dictionary<UnitBase, List<LinkVisual>> _bySource = new();
    private readonly Dictionary<(UnitBase, UnitBase), LinkVisual> _edgeIndex = new();
    private readonly HashSet<UnitBase> _knownUnits = new();

    private static readonly string[] TargetFieldNames =
        { "target", "currentTarget", "lockedTarget", "focusTarget", "attackTarget", "healTarget" };
    private static readonly string[] TargetsListNames =
        { "targets", "currentTargets", "lockedTargets", "focusTargets" };

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!lineMaterial)
        {
            var sh = Shader.Find("Sprites/Default") ?? Shader.Find("UI/Default");
            lineMaterial = new Material(sh);
        }

        SafeRescanUnits();
        TrySubscribeBSM();

        if (tryReflection) StartCoroutine(ReflectionLoop());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        foreach (var u in _knownUnits) UnhookUnit(u);

        var bsm = BattleSystemManager.Instance;
        if (bsm != null) bsm.UnitsChanged -= OnUnitsChangedBSM;

        foreach (var list in _bySource.Values)
            foreach (var v in list) if (v.lr) Destroy(v.lr.gameObject);
        _bySource.Clear();
        _edgeIndex.Clear();
        _knownUnits.Clear();
    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        if (!overlayEnabled) { HideAllLinesFast(); return; }

        float now = Time.time;
        List<UnitBase> toRemoveSource = null;

        foreach (var kv in _bySource)
        {
            var src = kv.Key;
            var list = kv.Value;

            if (!src)
            {
                if (toRemoveSource == null) toRemoveSource = new List<UnitBase>();
                toRemoveSource.Add(src);
                foreach (var v in list) { if (v.lr) Destroy(v.lr.gameObject); }
                continue;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var v = list[i];
                bool expired = (now - v.lastUpdateTime) > ttlSeconds || !v.target;

                if (expired)
                {
                    RemoveEdge(v);
                    list.RemoveAt(i);
                    continue;
                }

                bool teamVisible = (src.Team == TeamSide.Ally) ? showAllySources : showEnemySources;
                if (!teamVisible) { v.lr.enabled = false; continue; }

                Vector3 a = src.transform.position;
                Vector3 b = v.target.transform.position;
                v.lr.SetPosition(0, a);
                v.lr.SetPosition(1, b);

                float w = baseWidth * Mathf.Lerp(0.6f, 1.4f, v.weight);
                if (now < v.pulseUntil) w *= pulseWidthBoost;
                v.lr.startWidth = v.lr.endWidth = w;

                var col = GetColor(v.type, v.weight, now < v.pulseUntil, v.baseAlpha);
                v.lr.startColor = v.lr.endColor = col;

                v.lr.enabled = true;
            }
        }

        if (toRemoveSource != null)
            foreach (var s in toRemoveSource) _bySource.Remove(s);
    }

    public void SetOverlayEnabled(bool on)
    {
        overlayEnabled = on;
        if (!overlayEnabled) HideAllLinesFast();
    }
    public void ToggleOverlay() => SetOverlayEnabled(!overlayEnabled);

    public void SetAllyVisible(bool on)
    {
        showAllySources = on;
        if (!on) HideLinesByTeam(TeamSide.Ally);
    }
    public void SetEnemyVisible(bool on)
    {
        showEnemySources = on;
        if (!on) HideLinesByTeam(TeamSide.Enemy);
    }

    public void ClearAll()
    {
        foreach (var v in _edgeIndex.Values)
            if (v.lr) Destroy(v.lr.gameObject);
        _bySource.Clear();
        _edgeIndex.Clear();
    }

    private void SafeRescanUnits()
    {
        var list = CollectSceneUnits();

        var toRemove = _knownUnits.Where(u => u == null || !list.Contains(u)).ToList();
        foreach (var u in toRemove) { UnhookUnit(u); _knownUnits.Remove(u); }

        foreach (var u in list)
        {
            if (_knownUnits.Contains(u)) continue;
            HookUnit(u);
            _knownUnits.Add(u);
        }
    }

    private List<UnitBase> CollectSceneUnits()
    {
        var bsm = BattleSystemManager.Instance;
        if (bsm != null)
        {
            var l = new List<UnitBase>();
            if (bsm.AllyUnits != null) l.AddRange(bsm.AllyUnits.Where(u => u));
            if (bsm.EnemyUnits != null) l.AddRange(bsm.EnemyUnits.Where(u => u));
            return l;
        }
        return FindObjectsOfType<UnitBase>().ToList();
    }

    private void TrySubscribeBSM()
    {
        var bsm = BattleSystemManager.Instance;
        if (bsm != null)
        {
            bsm.UnitsChanged -= OnUnitsChangedBSM;
            bsm.UnitsChanged += OnUnitsChangedBSM;
        }

        if (autoScanUnits)
            StartCoroutine(AutoScanLoop());
    }

    private void OnUnitsChangedBSM()
    {
        SafeRescanUnits();
    }

    private IEnumerator AutoScanLoop()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            SafeRescanUnits();
            yield return wait;
        }
    }

    private void HookUnit(UnitBase u)
    {
        if (!u) return;

        u.OnBasicAttackStarted += OnBasicAttackStarted;
        u.OnBasicAttackHit += OnBasicAttackHit;
        u.OnSkillCastStarted += OnSkillCastStarted;
        u.OnSkillCastFinished += OnSkillCastFinished;
        u.OnDied += OnUnitDied;
    }

    private void UnhookUnit(UnitBase u)
    {
        if (!u) return;

        u.OnBasicAttackStarted -= OnBasicAttackStarted;
        u.OnBasicAttackHit -= OnBasicAttackHit;
        u.OnSkillCastStarted -= OnSkillCastStarted;
        u.OnSkillCastFinished -= OnSkillCastFinished;
        u.OnDied -= OnUnitDied;

        if (_bySource.TryGetValue(u, out var list))
        {
            foreach (var v in list) if (v.lr) Destroy(v.lr.gameObject);
            _bySource.Remove(u);
        }

        var keys = _edgeIndex.Keys.Where(k => k.Item1 == u || k.Item2 == u).ToList();
        foreach (var k in keys) _edgeIndex.Remove(k);
    }

    private void OnBasicAttackStarted(UnitBase self, GameObject tgtGO)
    {
        var tgt = ToUnit(tgtGO);
        if (!tgt) return;
        var type = Classify(self, tgt, preferAttack: true);
        EmitSingle(self, new LinkInfo(tgt, type, 1f), pulse: true);
    }

    private void OnBasicAttackHit(UnitBase self, GameObject tgtGO, float dmg)
    {
        var tgt = ToUnit(tgtGO);
        if (!tgt) return;
        var type = Classify(self, tgt, preferAttack: true);

        float w = Mathf.Clamp01(dmg > 0 ? 0.5f + Mathf.Log10(1f + dmg) * 0.25f : 0.5f);
        EmitSingle(self, new LinkInfo(tgt, type, w), pulse: true);
    }

    private void OnSkillCastStarted(UnitBase self, GameObject tgtGO)
    {
        var tgt = ToUnit(tgtGO);
        if (!tgt) return;
        var type = Classify(self, tgt, preferAttack: false);
        EmitSingle(self, new LinkInfo(tgt, type, 0.9f), pulse: false);
    }

    private void OnSkillCastFinished(UnitBase self, GameObject tgtGO)
    {
        var tgt = ToUnit(tgtGO);
        if (!tgt) return;
        var type = Classify(self, tgt, preferAttack: false);
        EmitSingle(self, new LinkInfo(tgt, type, 0.8f), pulse: true);
    }

    private void OnUnitDied(UnitBase self)
    {
        if (_bySource.TryGetValue(self, out var list))
        {
            foreach (var v in list) if (v.lr) Destroy(v.lr.gameObject);
            _bySource.Remove(self);
        }
        var keys = _edgeIndex.Keys.Where(k => k.Item1 == self).ToList();
        foreach (var k in keys) _edgeIndex.Remove(k);
    }

    private IEnumerator ReflectionLoop()
    {
        var wait = new WaitForSeconds(reflectInterval);
        while (true)
        {
            if (overlayEnabled)
            {
                foreach (var u in _knownUnits)
                {
                    if (!u) continue;

                    var links = TryReflectLinks(u);
                    if (links != null && links.Count > 0)
                        EmitLinks(u, links);
                }
            }
            yield return wait;
        }
    }

    private List<LinkInfo> TryReflectLinks(UnitBase src)
    {
        var comps = src.GetComponents<Component>();
        var results = new HashSet<UnitBase>();

        foreach (var c in comps)
        {
            if (!c || c == this) continue;
            var t = c.GetType();
            string tn = t.Name.ToLowerInvariant();

            bool likely = tn.Contains("target") || tn.Contains("ai") || tn.Contains("combat") || tn.Contains("attack");
            if (!likely) continue;

            // 단일
            foreach (var name in TargetFieldNames)
                ReadTargetMember(c, t, name, results);

            // 다중
            foreach (var name in TargetsListNames)
                ReadTargetsMember(c, t, name, results);
        }

        if (results.Count == 0) return null;

        var list = new List<LinkInfo>(results.Count);
        foreach (var tgt in results)
        {
            if (!tgt) continue;
            var type = Classify(src, tgt, preferAttack: true);
            list.Add(new LinkInfo(tgt, type, 0.8f));
        }
        return list;
    }

    private void ReadTargetMember(Component c, Type t, string name, HashSet<UnitBase> outSet)
    {
        var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (f != null) { var ub = CoerceToUnit(f.GetValue(c)); if (ub) outSet.Add(ub); }

        var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (p != null && p.CanRead) { var ub = CoerceToUnit(p.GetValue(c)); if (ub) outSet.Add(ub); }
    }

    private void ReadTargetsMember(Component c, Type t, string name, HashSet<UnitBase> outSet)
    {
        var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (f != null) CoerceToUnits(f.GetValue(c), outSet);

        var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (p != null && p.CanRead) CoerceToUnits(p.GetValue(c), outSet);
    }

    private void EmitSingle(UnitBase src, LinkInfo link, bool pulse)
        => EmitLinks(src, link.target ? new List<LinkInfo> { link } : null, pulse ? link.target : null, pulse ? link.type : DebugLinkType.Attack, link.weight);

    private void EmitLinks(UnitBase src, IReadOnlyList<LinkInfo> links,
                           UnitBase pulseTgt = null, DebugLinkType pulseType = DebugLinkType.Attack, float pulseWeight = 1f)
    {
        if (!src) return;

        if ((src.Team == TeamSide.Ally && !showAllySources) ||
            (src.Team == TeamSide.Enemy && !showEnemySources))
        {
            HideAllFor(src);
            return;
        }

        if (!_bySource.TryGetValue(src, out var list))
        {
            list = new List<LinkVisual>(8);
            _bySource[src] = list;
        }

        var active = new HashSet<UnitBase>();
        if (links != null)
        {
            foreach (var li in links)
            {
                if (!li.target) continue;
                active.Add(li.target);
                var key = (src, li.target);

                if (!_edgeIndex.TryGetValue(key, out var vis))
                {
                    vis = CreateEdge(src, li.target, li.type, li.weight);
                    list.Add(vis);
                    _edgeIndex[key] = vis;
                }
                else
                {
                    vis.type = li.type;
                    vis.weight = li.weight;
                }
                Touch(vis);
            }
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var v = list[i];
            if (!v.target || (active.Count > 0 && !active.Contains(v.target)))
                v.lastUpdateTime = Mathf.Min(v.lastUpdateTime, Time.time - ttlSeconds * 0.9f);
        }

        if (pulseTgt)
        {
            var key = (src, pulseTgt);
            if (_edgeIndex.TryGetValue(key, out var vis))
            {
                vis.pulseUntil = Time.time + pulseDuration;
                vis.type = pulseType;
                vis.weight = Mathf.Max(vis.weight, pulseWeight);
                Touch(vis);
            }
            else
            {
                var v = CreateEdge(src, pulseTgt, pulseType, pulseWeight);
                v.pulseUntil = Time.time + pulseDuration;
                Touch(v);
                list.Add(v);
                _edgeIndex[key] = v;
            }
        }
    }

    private LinkVisual CreateEdge(UnitBase src, UnitBase tgt, DebugLinkType type, float weight)
    {
        var go = new GameObject($"[Link] {src?.name} -> {tgt?.name}");
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.material = lineMaterial;
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCapVertices = 6;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingOrder = 5000;
        lr.enabled = false;

        var v = new LinkVisual
        {
            lr = lr,
            source = src,
            target = tgt,
            type = type,
            weight = Mathf.Clamp01(weight),
            baseAlpha = 0.9f,
            lastUpdateTime = Time.time
        };
        return v;
    }

    private void Touch(LinkVisual v)
    {
        v.lastUpdateTime = Time.time;
        if (overlayEnabled && v.lr) v.lr.enabled = true;
    }

    private void RemoveEdge(LinkVisual v)
    {
        if (v == null) return;
        var key = (v.source, v.target);
        _edgeIndex.Remove(key);
        if (v.lr) Destroy(v.lr.gameObject);
    }

    private void HideAllLinesFast()
    {
        foreach (var v in _edgeIndex.Values) if (v.lr) v.lr.enabled = false;
    }

    private void HideLinesByTeam(TeamSide team)
    {
        foreach (var v in _edgeIndex.Values)
            if (v.source && v.source.Team == team && v.lr) v.lr.enabled = false;
    }

    private void HideAllFor(UnitBase src)
    {
        if (!_bySource.TryGetValue(src, out var list)) return;
        foreach (var v in list) { if (v.lr) v.lr.enabled = false; v.lastUpdateTime = -999f; }
    }

    private Color GetColor(DebugLinkType t, float weight, bool pulsing, float baseA)
    {
        Color c = t switch
        {
            DebugLinkType.Attack => new Color(1.0f, 0.35f, 0.2f, 1.0f),
            DebugLinkType.Heal => new Color(0.15f, 1.0f, 0.6f, 1.0f),
            DebugLinkType.Buff => new Color(0.35f, 0.6f, 1.0f, 1.0f),
            DebugLinkType.Debuff => new Color(0.9f, 0.25f, 1.0f, 1.0f),
            DebugLinkType.Support => new Color(1.0f, 0.85f, 0.25f, 1.0f),
            _ => Color.white
        };
        float a = Mathf.Lerp(0.35f, baseA, Mathf.Clamp01(weight * 1.2f));
        if (pulsing) a = 1.0f;
        c.a = a;
        return c;
    }

    private static UnitBase ToUnit(GameObject go)
    {
        if (!go) return null;
        var ub = go.GetComponent<UnitBase>() ?? go.GetComponentInParent<UnitBase>();
        return ub;
    }

    private static UnitBase CoerceToUnit(object obj)
    {
        if (obj == null) return null;
        switch (obj)
        {
            case UnitBase ub: return ub;
            case GameObject go: return ToUnit(go);
            case Component cp: return ToUnit(cp.gameObject);
            default: return null;
        }
    }

    private static void CoerceToUnits(object obj, HashSet<UnitBase> outSet)
    {
        if (obj == null) return;
        if (obj is IEnumerable enumerable)
        {
            foreach (var it in enumerable)
            {
                var ub = CoerceToUnit(it);
                if (ub) outSet.Add(ub);
            }
        }
        else
        {
            var ub = CoerceToUnit(obj);
            if (ub) outSet.Add(ub);
        }
    }

    private static DebugLinkType Classify(UnitBase src, UnitBase tgt, bool preferAttack)
    {
        if (!src || !tgt) return DebugLinkType.Attack;
        bool sameTeam = src.Team == tgt.Team && src.Team != TeamSide.Neutral;

        if (sameTeam) return preferAttack ? DebugLinkType.Support : DebugLinkType.Heal;
        else return DebugLinkType.Attack;
    }
}
