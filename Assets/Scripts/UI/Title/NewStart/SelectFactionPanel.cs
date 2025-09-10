using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectFactionPanel : MonoBehaviour
{
    [SerializeField]
    BarrackUnitDetailPanel m_unitDetailPanel = null;

    [SerializeField]
    Image m_factionImage = null;

    [SerializeField]
    TextMeshProUGUI m_factionNameText = null;

    [SerializeField]
    TextMeshProUGUI m_factionExplainText = null;

    [SerializeField]
    GameObject m_selectFacitonBtnPrefeb = null;

    [SerializeField]
    GameObject m_unitBtnPrefeb = null;

    [SerializeField]
    Transform m_selectFactionBtnContent = null;

    [SerializeField]
    Transform m_unitBtnContent = null;

    public void Init()
    {
        // 기존 자식 정리
        ClearChildren(m_selectFactionBtnContent);
        ClearChildren(m_unitBtnContent);

        // GameDataManager에서 팩션 목록을 가져와 버튼 생성
        var factionDict = GameDataManager.Instance != null ? GameDataManager.Instance.FactionEntryDict : null;
        FactionData firstFactionData = null;
        
        if (factionDict != null)
        {
            foreach (var kv in factionDict)
            {
                var factionEntry = kv.Value;
                if (factionEntry == null) continue;
                // FactionEntry는 보통 m_data에 원본 FactionData를 보관하는 패턴을 사용
                var factionDataField = factionEntry.GetType().GetField("m_data");
                FactionData factionData = factionDataField != null ? (FactionData)factionDataField.GetValue(factionEntry) : null;
                if (factionData == null) continue;

                // None 팩션은 제외
                if (factionData.m_factionType == FactionType.TYPE.None) continue;

                // 첫 번째 팩션 데이터 저장
                if (firstFactionData == null)
                    firstFactionData = factionData;

                var btnObj = Instantiate(m_selectFacitonBtnPrefeb, m_selectFactionBtnContent);
                var btn = btnObj.GetComponent<SelectFactionBtn>();
                if (btn != null)
                {
                    // 버튼 초기화 및 클릭 시 이 패널로 콜백
                    btn.Initialize(factionData, this);
                }
            }
        }

        // 첫 번째 팩션 정보를 자동으로 표시
        if (firstFactionData != null)
        {
            ShowFactionInfo(firstFactionData);
        }

        gameObject.SetActive(true);
    }

    public void ShowFactionInfo(FactionData argFactionData)
    {
        if (argFactionData == null) return;

        // 헤더 텍스트/이미지 갱신 (안전한 범위에서만)
        if (m_factionNameText != null)
            m_factionNameText.text = argFactionData.name;

        // 설명 텍스트 설정
        if (m_factionExplainText != null)
            m_factionExplainText.text = argFactionData.m_description;

        // 팩션 아이콘 이미지 설정
        if (m_factionImage != null && argFactionData.m_icon != null)
            m_factionImage.sprite = argFactionData.m_icon;

        // 유닛 버튼 목록 갱신 (4개만 선택)
        ClearChildren(m_unitBtnContent);
        if (argFactionData.m_units != null && argFactionData.m_units.Count > 0)
        {
            var selectedUnits = SelectRepresentativeUnits(argFactionData.m_units, 4);
            
            foreach (var unit in selectedUnits)
            {
                if (unit == null) continue;
                var unitBtnObj = Instantiate(m_unitBtnPrefeb, m_unitBtnContent);
                var unitBtn = unitBtnObj.GetComponent<SelectFactionUnitBtn>();
                if (unitBtn != null)
                {
                    unitBtn.Initialize(unit, this);
                }
            }
        }
    }

    public void ShowUnitDetailPanel(UnitData argUnitData)
    {
        m_unitDetailPanel.Init(argUnitData);
    }

    private void ClearChildren(Transform content)
    {
        if (content == null) return;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// 대표 유닛들을 선택합니다. (1티어 2개, 2티어 1개, 3티어 1개 또는 무작위 4개)
    /// </summary>
    /// <param name="allUnits">모든 유닛 리스트</param>
    /// <param name="count">선택할 유닛 수</param>
    /// <returns>선택된 유닛 리스트</returns>
    private List<UnitData> SelectRepresentativeUnits(List<UnitData> allUnits, int count)
    {
        if (allUnits == null || allUnits.Count == 0) return new List<UnitData>();
        
        // 유닛이 4개 이하면 모두 반환
        if (allUnits.Count <= count)
            return new List<UnitData>(allUnits);

        // 공격력 + 체력을 기준으로 티어 분류 시도
        var tieredUnits = TryClassifyByTier(allUnits);
        
        if (tieredUnits != null)
        {
            // 티어별로 선택: 1티어 2개, 2티어 1개, 3티어 1개
            var selected = new List<UnitData>();
            
            // 1티어에서 2개 선택
            if (tieredUnits.Count > 0 && tieredUnits[0].Count > 0)
            {
                selected.AddRange(SelectRandomUnits(tieredUnits[0], Mathf.Min(2, tieredUnits[0].Count)));
            }
            
            // 2티어에서 1개 선택
            if (tieredUnits.Count > 1 && tieredUnits[1].Count > 0 && selected.Count < count)
            {
                selected.AddRange(SelectRandomUnits(tieredUnits[1], 1));
            }
            
            // 3티어에서 1개 선택
            if (tieredUnits.Count > 2 && tieredUnits[2].Count > 0 && selected.Count < count)
            {
                selected.AddRange(SelectRandomUnits(tieredUnits[2], 1));
            }
            
            // 부족한 만큼 무작위로 채우기
            if (selected.Count < count)
            {
                var remaining = allUnits.Where(u => !selected.Contains(u)).ToList();
                selected.AddRange(SelectRandomUnits(remaining, count - selected.Count));
            }
            
            return selected;
        }
        else
        {
            // 티어 분류가 불가능하면 무작위로 4개 선택
            return SelectRandomUnits(allUnits, count);
        }
    }

    /// <summary>
    /// 공격력 + 체력을 기준으로 유닛을 3개 티어로 분류합니다.
    /// </summary>
    /// <param name="units">분류할 유닛 리스트</param>
    /// <returns>3개 티어로 분류된 리스트, 분류 불가능하면 null</returns>
    private List<List<UnitData>> TryClassifyByTier(List<UnitData> units)
    {
        if (units == null || units.Count < 3) return null;
        
        // 공격력 + 체력으로 정렬
        var sortedUnits = units.OrderBy(u => u.attackPower + u.maxHealth).ToList();
        
        // 3개 그룹으로 나누기
        var tier1 = new List<UnitData>(); // 하위 33%
        var tier2 = new List<UnitData>(); // 중위 33%
        var tier3 = new List<UnitData>(); // 상위 34%
        
        int groupSize = sortedUnits.Count / 3;
        int remainder = sortedUnits.Count % 3;
        
        int index = 0;
        
        // 1티어 (하위)
        for (int i = 0; i < groupSize + (remainder > 0 ? 1 : 0); i++)
        {
            tier1.Add(sortedUnits[index++]);
        }
        remainder--;
        
        // 2티어 (중위)
        for (int i = 0; i < groupSize + (remainder > 0 ? 1 : 0); i++)
        {
            tier2.Add(sortedUnits[index++]);
        }
        remainder--;
        
        // 3티어 (상위)
        for (int i = index; i < sortedUnits.Count; i++)
        {
            tier3.Add(sortedUnits[i]);
        }
        
        return new List<List<UnitData>> { tier1, tier2, tier3 };
    }

    /// <summary>
    /// 리스트에서 무작위로 지정된 수만큼 유닛을 선택합니다.
    /// </summary>
    /// <param name="units">선택할 유닛 리스트</param>
    /// <param name="count">선택할 수</param>
    /// <returns>선택된 유닛 리스트</returns>
    private List<UnitData> SelectRandomUnits(List<UnitData> units, int count)
    {
        if (units == null || units.Count == 0 || count <= 0) return new List<UnitData>();
        
        var available = new List<UnitData>(units);
        var selected = new List<UnitData>();
        
        for (int i = 0; i < Mathf.Min(count, available.Count); i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            selected.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
        
        return selected;
    }
}
