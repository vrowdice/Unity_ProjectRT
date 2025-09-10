using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class BarrackPanel : BasePanel
{
    private const int m_barrackBuildingCode = 10003;
    [SerializeField]
    GameObject m_factionUnitPanelPrefab;
    [SerializeField]
    GameObject m_factionTogglePrefab;

    [SerializeField]
    Transform m_unitScrollViewContentTrans;
    [SerializeField]
    Transform m_factionToggleContentTrans;

    [Header("Unit Detail Panel Reference")]
    [SerializeField]
    private BarrackUnitDetailPanel m_unitDetailPanel;

    [Header("Unit Type Filter Toggles")]
    [SerializeField]
    Toggle m_meleeFilterToggle;
    [SerializeField]
    Toggle m_rangeFilterToggle;
    [SerializeField]
    Toggle m_defenseFilterToggle;
    [SerializeField]
    Toggle m_allTypeFilterToggle;
    [SerializeField]
    ToggleGroup m_unitTypeToggleGroup;

    // 필터링 상태 관리
    private FactionType.TYPE m_currentSelectedFaction = FactionType.TYPE.None;
    private UnitTagType? m_currentSelectedUnitType = null;
    private List<BarrackFactionUnitPanel> m_activeFactionPanels = new List<BarrackFactionUnitPanel>();
    private List<BarrackFactionToggle> m_activeFactionToggles = new List<BarrackFactionToggle>();

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Barrack");
        SetBuildingLevel(m_barrackBuildingCode);

        // 여기에 병영 패널 특화 초기화 로직을 추가할 수 있습니다
        InitializeBarrackPanel();
    }

    /// <summary>
    /// 병영 패널 초기화
    /// </summary>
    private void InitializeBarrackPanel()
    {
        // 기존 UI 요소들 정리
        ClearExistingUI();
        
        // 필터 초기화
        InitializeFilters();
        
        // 팩션 토글과 유닛 패널 초기화
        InitFactionToggle();
        InitFactionUnitPanel();
        
        // 초기 상태: 모든 팩션, 모든 유닛 타입 표시
        ShowAllFactions();
        UpdateUnitTypeFilterToggles();
    }

    /// <summary>
    /// 필터 시스템 초기화
    /// </summary>
    private void InitializeFilters()
    {
        // 유닛 타입 필터 초기화
        InitializeUnitTypeFilters();
    }

    /// <summary>
    /// 유닛 타입 필터 토글 시스템 초기화
    /// </summary>
    private void InitializeUnitTypeFilters()
    {
        // 토글 그룹 설정
        if (m_unitTypeToggleGroup != null)
        {
            if (m_meleeFilterToggle != null)
                m_meleeFilterToggle.group = m_unitTypeToggleGroup;
            if (m_rangeFilterToggle != null)
                m_rangeFilterToggle.group = m_unitTypeToggleGroup;
            if (m_defenseFilterToggle != null)
                m_defenseFilterToggle.group = m_unitTypeToggleGroup;
            if (m_allTypeFilterToggle != null)
                m_allTypeFilterToggle.group = m_unitTypeToggleGroup;
        }

        // 필터 토글 이벤트 설정
        if (m_meleeFilterToggle != null)
            m_meleeFilterToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) OnUnitTypeFilterToggle(UnitTagType.Melee);
            });
        if (m_rangeFilterToggle != null)
            m_rangeFilterToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) OnUnitTypeFilterToggle(UnitTagType.Range);
            });
        if (m_defenseFilterToggle != null)
            m_defenseFilterToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) OnUnitTypeFilterToggle(UnitTagType.Defense);
            });
        if (m_allTypeFilterToggle != null)
            m_allTypeFilterToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) OnUnitTypeFilterToggle(null);
            });

        // 초기 유닛 타입 필터 상태 (전체 선택)
        m_currentSelectedUnitType = null;
        
        // 전체 토글을 기본으로 선택
        if (m_allTypeFilterToggle != null)
            m_allTypeFilterToggle.isOn = true;
    }

    /// <summary>
    /// 기존 UI 요소들을 정리합니다
    /// </summary>
    private void ClearExistingUI()
    {
        // 기존 팩션 토글들 제거
        foreach (Transform child in m_factionToggleContentTrans)
        {
            Destroy(child.gameObject);
        }

        // 기존 유닛 패널들 제거
        foreach (Transform child in m_unitScrollViewContentTrans)
        {
            Destroy(child.gameObject);
        }

        // 리스트 초기화
        m_activeFactionPanels.Clear();
        m_activeFactionToggles.Clear();
    }

    /// <summary>
    /// 팩션 토글 초기화 (우호도 1 이상인 팩션만)
    /// </summary>
    private void InitFactionToggle()
    {
        foreach (var factionEntry in m_gameDataManager.FactionEntryDict)
        {
            // 우호도가 1 이상인 팩션만 토글 생성
            if (factionEntry.Value.m_state.m_like >= 1)
            {
                GameObject factionToggle = Instantiate(m_factionTogglePrefab, m_factionToggleContentTrans);
                BarrackFactionToggle toggleComponent = factionToggle.GetComponent<BarrackFactionToggle>();
                toggleComponent.Init(factionEntry.Value.m_data);
                
                // 팩션 선택 콜백 설정
                toggleComponent.SetOnFactionSelected(OnFactionSelected);
                
                m_activeFactionToggles.Add(toggleComponent);
            }
        }
    }

    /// <summary>
    /// 팩션별 유닛 패널 초기화
    /// </summary>
    private void InitFactionUnitPanel()
    {
        foreach (var factionEntry in m_gameDataManager.FactionEntryDict)
        {
            // 우호도가 1 이상이고 유닛이 있는 팩션만 패널 생성
            if (factionEntry.Value.m_state.m_like >= 1 && factionEntry.Value.m_data.m_units.Count > 0)
            {
                GameObject factionUnitPanel = Instantiate(m_factionUnitPanelPrefab, m_unitScrollViewContentTrans);
                BarrackFactionUnitPanel panelComponent = factionUnitPanel.GetComponent<BarrackFactionUnitPanel>();
                panelComponent.Init(factionEntry.Value.m_data, this);
                
                m_activeFactionPanels.Add(panelComponent);
            }
        }
    }

    /// <summary>
    /// BarrackUnitDetailPanel로 유닛 상세를 오픈
    /// </summary>
    public void ShowUnitDetail(UnitData unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("ShowUnitDetail called with null unit");
            return;
        }
        if (m_unitDetailPanel != null)
        {
            m_unitDetailPanel.Init(unit);
        }
        else
        {
            Debug.LogWarning("m_unitDetailPanel is not assigned in inspector");
        }
    }

    /// <summary>
    /// 팩션이 선택되었을 때 호출되는 콜백
    /// </summary>
    /// <param name="factionType">선택된 팩션 타입</param>
    public void OnFactionSelected(FactionType.TYPE factionType)
    {
        m_currentSelectedFaction = factionType;
        
        // 선택된 팩션에 따라 유닛 패널 필터링
        FilterFactionPanels();
        
        // 유닛 타입 필터 토글 상태 업데이트
        UpdateUnitTypeFilterToggles();
    }

    /// <summary>
    /// 유닛 타입 필터 토글이 변경되었을 때 호출
    /// </summary>
    /// <param name="unitType">선택된 유닛 타입 (null이면 전체)</param>
    public void OnUnitTypeFilterToggle(UnitTagType? unitType)
    {
        m_currentSelectedUnitType = unitType;
        
        // 현재 표시 중인 팩션들의 유닛을 타입별로 필터링
        FilterUnitsByType();
    }

    /// <summary>
    /// 팩션 필터에 따라 유닛 패널들을 표시/숨김
    /// </summary>
    private void FilterFactionPanels()
    {
        foreach (var panel in m_activeFactionPanels)
        {
            bool shouldShow = (m_currentSelectedFaction == FactionType.TYPE.None) || 
                             (panel.GetFactionType() == m_currentSelectedFaction);
            
            panel.gameObject.SetActive(shouldShow);
        }
    }

    /// <summary>
    /// 유닛 타입에 따라 유닛 버튼들을 필터링
    /// </summary>
    private void FilterUnitsByType()
    {
        foreach (var panel in m_activeFactionPanels)
        {
            if (panel.gameObject.activeInHierarchy)
            {
                panel.FilterUnitsByType(m_currentSelectedUnitType);
            }
        }
    }

    /// <summary>
    /// 모든 팩션을 표시
    /// </summary>
    private void ShowAllFactions()
    {
        m_currentSelectedFaction = FactionType.TYPE.None;
        FilterFactionPanels();
    }

    /// <summary>
    /// 유닛 타입 필터 토글들의 활성화/비활성화 상태 업데이트
    /// </summary>
    private void UpdateUnitTypeFilterToggles()
    {
        // 현재 선택된 팩션에서 사용 가능한 유닛 타입들을 확인
        HashSet<UnitTagType> availableTypes = GetAvailableUnitTypesForCurrentFaction();
        
        // 각 토글의 활성화 상태 설정
        if (m_meleeFilterToggle != null)
            m_meleeFilterToggle.interactable = availableTypes.Contains(UnitTagType.Melee);
        if (m_rangeFilterToggle != null)
            m_rangeFilterToggle.interactable = availableTypes.Contains(UnitTagType.Range);
        if (m_defenseFilterToggle != null)
            m_defenseFilterToggle.interactable = availableTypes.Contains(UnitTagType.Defense);
        if (m_allTypeFilterToggle != null)
            m_allTypeFilterToggle.interactable = true; // 항상 활성화
            
        // 현재 선택된 타입이 사용 불가능하면 전체로 변경
        if (m_currentSelectedUnitType.HasValue && 
            !availableTypes.Contains(m_currentSelectedUnitType.Value) &&
            m_allTypeFilterToggle != null)
        {
            m_allTypeFilterToggle.isOn = true;
        }
    }

    /// <summary>
    /// 현재 선택된 팩션에서 사용 가능한 유닛 타입들을 반환
    /// </summary>
    /// <returns>사용 가능한 유닛 타입 집합</returns>
    private HashSet<UnitTagType> GetAvailableUnitTypesForCurrentFaction()
    {
        HashSet<UnitTagType> availableTypes = new HashSet<UnitTagType>();
        
        if (m_currentSelectedFaction == FactionType.TYPE.None)
        {
            // 모든 팩션이 선택된 경우, 모든 사용 가능한 타입 반환
            foreach (var factionEntry in m_gameDataManager.FactionEntryDict)
            {
                if (factionEntry.Value.m_state.m_like >= 1)
                {
                    foreach (var unit in factionEntry.Value.m_data.m_units)
                    {
                        availableTypes.Add(unit.unitTagType);
                    }
                }
            }
        }
        else
        {
            // 특정 팩션이 선택된 경우, 해당 팩션의 유닛 타입들만 반환
            if (m_gameDataManager.FactionEntryDict.TryGetValue(m_currentSelectedFaction, out FactionEntry factionEntry))
            {
                foreach (var unit in factionEntry.m_data.m_units)
                {
                    availableTypes.Add(unit.unitTagType);
                }
            }
        }
        
        return availableTypes;
    }

    /// <summary>
    /// 특정 팩션의 유닛들을 타입별로 분류하여 반환합니다
    /// </summary>
    /// <param name="factionType">분류할 팩션 타입</param>
    /// <returns>타입별로 분류된 유닛 딕셔너리</returns>
    public Dictionary<UnitTagType, List<UnitData>> GetUnitsByType(FactionType.TYPE factionType)
    {
        Dictionary<UnitTagType, List<UnitData>> unitsByType = new Dictionary<UnitTagType, List<UnitData>>
        {
            { UnitTagType.Melee, new List<UnitData>() },
            { UnitTagType.Range, new List<UnitData>() },
            { UnitTagType.Defense, new List<UnitData>() }
        };

        if (m_gameDataManager.FactionEntryDict.TryGetValue(factionType, out FactionEntry factionEntry))
        {
            foreach (UnitData unit in factionEntry.m_data.m_units)
            {
                if (unitsByType.ContainsKey(unit.unitTagType))
                {
                    unitsByType[unit.unitTagType].Add(unit);
                }
            }
        }

        return unitsByType;
    }

    /// <summary>
    /// 모든 사용 가능한 팩션의 유닛들을 타입별로 분류하여 반환합니다
    /// </summary>
    /// <returns>타입별로 분류된 전체 유닛 딕셔너리</returns>
    public Dictionary<UnitTagType, List<UnitData>> GetAllAvailableUnitsByType()
    {
        Dictionary<UnitTagType, List<UnitData>> allUnitsByType = new Dictionary<UnitTagType, List<UnitData>>
        {
            { UnitTagType.Melee, new List<UnitData>() },
            { UnitTagType.Range, new List<UnitData>() },
            { UnitTagType.Defense, new List<UnitData>() }
        };

        foreach (var factionEntry in m_gameDataManager.FactionEntryDict)
        {
            // 우호도가 1 이상인 팩션의 유닛만 포함
            if (factionEntry.Value.m_state.m_like >= 1)
            {
                foreach (UnitData unit in factionEntry.Value.m_data.m_units)
                {
                    if (allUnitsByType.ContainsKey(unit.unitTagType))
                    {
                        allUnitsByType[unit.unitTagType].Add(unit);
                    }
                }
            }
        }

        return allUnitsByType;
    }
}
