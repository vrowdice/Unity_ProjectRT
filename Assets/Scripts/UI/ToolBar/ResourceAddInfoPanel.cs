using UnityEngine;
using System.Collections.Generic;

public class ResourceAddInfoPanel : MonoBehaviour
{
    [SerializeField]
    ResourceAddInfoPanelContent m_territoryContent = null;
    [SerializeField]
    ResourceAddInfoPanelContent m_buildingContent = null;

    public void Init(List<EffectBase> argEffectBases, ResourceType.TYPE argType)
    {
        List<string> buildingTexts = new List<string>();
        List<string> territoryTexts = new List<string>();

        // 건물 기본 생산량을 건물 섹션 첫째 줄에 추가
        long buildingAmount = GameManager.Instance.GetBuildingProduction(argType);
        if (buildingAmount > 0)
        {
            string buildingBaseText = $"Base Production: +{ReplaceUtils.FormatNumber(buildingAmount)}";
            buildingTexts.Add(buildingBaseText);
        }

        // 영토 기본 생산량을 영토 섹션 첫째 줄에 추가
        long territoryAmount = GameManager.Instance.GetTerritoryProduction(argType);
        if (territoryAmount > 0)
        {
            string territoryBaseText = $"Base Production: +{ReplaceUtils.FormatNumber(territoryAmount)}";
            territoryTexts.Add(territoryBaseText);
        }

        foreach (EffectBase effect in argEffectBases)
        {
            // 리소스 관련 통합 이펙트만 처리
            if (effect is UnifiedResourceEffectMod resMod)
            {
                // 해당 리소스 타입에 영향을 주는지 확인
                bool affectsThisResource = resMod.m_scopeType == EffectScopeType.TYPE.All || 
                                         resMod.m_resourceType == argType;
                
                if (!affectsThisResource)
                {
                    continue;
                }

                // 이벤트명과 수치 정보 생성
                string eventName = string.IsNullOrEmpty(effect.ActivatedEventName) ? 
                                 effect.m_name : effect.ActivatedEventName;
                string amountText = GetAmountText(resMod.m_operationType, resMod.m_modifier);
                string displayText = $"{eventName}: {amountText}";

                // 타겟에 따라 분류
                switch (resMod.m_targetType)
                {
                    case EffectTargetType.TYPE.Building:
                        buildingTexts.Add(displayText);
                        break;
                    case EffectTargetType.TYPE.Territory:
                        territoryTexts.Add(displayText);
                        break;
                    case EffectTargetType.TYPE.Both:
                        buildingTexts.Add(displayText);
                        territoryTexts.Add(displayText);
                        break;
                }
            }
        }

        // UI 업데이트
        UpdateContent(m_buildingContent, buildingTexts, "Building");
        UpdateContent(m_territoryContent, territoryTexts, "Territory");
    }

    private void UpdateContent(ResourceAddInfoPanelContent content, List<string> texts, string title)
    {
        if (content != null)
        {
            bool hasContent = texts.Count > 0;
            content.gameObject.SetActive(hasContent);
            
            if (hasContent)
            {
                content.Init(texts, title);
            }
        }
    }

    private string GetAmountText(EffectOperationType.TYPE opType, float modifier)
    {
        switch (opType)
        {
            case EffectOperationType.TYPE.Multiply:
                return $"{modifier:0.##}x";
            case EffectOperationType.TYPE.Add:
                // 정수인 경우 소수점 제거
                if (Mathf.Approximately(modifier, Mathf.Round(modifier)))
                {
                    return $"+{Mathf.RoundToInt(modifier)}";
                }
                return $"+{modifier:0.##}";
            default:
                return modifier.ToString("0.##");
        }
    }
}