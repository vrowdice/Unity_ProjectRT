using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnifiedResourceEffectMod", menuName = "Event Effects/Resource/Unified Resource Mod")]
public class UnifiedResourceEffectMod : EffectBase
{
    [Header("Effect Configuration")]
    public EffectTargetType.TYPE m_targetType = EffectTargetType.TYPE.Building;
    public EffectScopeType.TYPE m_scopeType = EffectScopeType.TYPE.Specific;
    public EffectOperationType.TYPE m_operationType = EffectOperationType.TYPE.Multiply;
    public ResourceType.TYPE m_resourceType = ResourceType.TYPE.Wood;
    public float m_modifier = 1.2f; // 배수 (1.2 = 20% 증가) 또는 고정값, 합연산일 경우 정수로 20이면 20% 합연산 증가

    // 적용된 리소스 타입들을 추적 (중복 방지용)
    private HashSet<ResourceType.TYPE> m_appliedResources = new HashSet<ResourceType.TYPE>();

    public override void Activate(GameDataManager argDataManager, string argEventName)
    {
        m_appliedResources.Clear(); // 새로운 활성화 시 추적 초기화

        if (m_scopeType == EffectScopeType.TYPE.All)
        {
            // 모든 리소스에 적용
            foreach (ResourceType.TYPE type in System.Enum.GetValues(typeof(ResourceType.TYPE)))
            {
                if (ApplyModifier(argDataManager, type))
                {
                    m_appliedResources.Add(type);
                }
            }
        }
        else
        {
            // 특정 리소스에만 적용
            if (ApplyModifier(argDataManager, m_resourceType))
            {
                m_appliedResources.Add(m_resourceType);
            }
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        // 적용된 리소스들만 제거
        foreach (ResourceType.TYPE resourceType in m_appliedResources)
        {
            RemoveModifier(argDataManager, resourceType);
        }
        
        m_appliedResources.Clear();
    }

    private bool ApplyModifier(GameDataManager argDataManager, ResourceType.TYPE resourceType)
    {
        switch (m_targetType)
        {
            case EffectTargetType.TYPE.Building:
                return ApplyOperation(argDataManager.EventState.m_buildingResourceModDic, 
                                   argDataManager.EventState.m_buildingResourceAddDic, resourceType, m_modifier);
            case EffectTargetType.TYPE.Territory:
                return ApplyOperation(argDataManager.EventState.m_territoryResourceModDic, 
                                   argDataManager.EventState.m_territoryResourceAddDic, resourceType, m_modifier);
            case EffectTargetType.TYPE.Both:
                bool buildingResult = ApplyOperation(argDataManager.EventState.m_buildingResourceModDic, 
                                                   argDataManager.EventState.m_buildingResourceAddDic, resourceType, m_modifier);
                bool territoryResult = ApplyOperation(argDataManager.EventState.m_territoryResourceModDic, 
                                                    argDataManager.EventState.m_territoryResourceAddDic, resourceType, m_modifier);
                return buildingResult || territoryResult;
            default:
                return false;
        }
    }

    private void RemoveModifier(GameDataManager argDataManager, ResourceType.TYPE resourceType)
    {
        switch (m_targetType)
        {
            case EffectTargetType.TYPE.Building:
                RemoveOperation(argDataManager.EventState.m_buildingResourceModDic, 
                              argDataManager.EventState.m_buildingResourceAddDic, resourceType, m_modifier);
                break;
            case EffectTargetType.TYPE.Territory:
                RemoveOperation(argDataManager.EventState.m_territoryResourceModDic, 
                              argDataManager.EventState.m_territoryResourceAddDic, resourceType, m_modifier);
                break;
            case EffectTargetType.TYPE.Both:
                RemoveOperation(argDataManager.EventState.m_buildingResourceModDic, 
                              argDataManager.EventState.m_buildingResourceAddDic, resourceType, m_modifier);
                RemoveOperation(argDataManager.EventState.m_territoryResourceModDic, 
                              argDataManager.EventState.m_territoryResourceAddDic, resourceType, m_modifier);
                break;
        }
    }

    private bool ApplyOperation(Dictionary<ResourceType.TYPE, float> modDic, Dictionary<ResourceType.TYPE, float> addDic, 
                              ResourceType.TYPE resourceType, float modifier)
    {
        switch (m_operationType)
        {
            case EffectOperationType.TYPE.Multiply:
                if (!modDic.ContainsKey(resourceType))
                {
                    modDic[resourceType] = 1.0f; // 기본값 설정
                }
                modDic[resourceType] *= modifier;
                return true;
            case EffectOperationType.TYPE.Add:
                if (!addDic.ContainsKey(resourceType))
                {
                    addDic[resourceType] = 0.0f; // 기본값 설정
                }
                addDic[resourceType] += modifier;
                return true;
            default:
                return false;
        }
    }

    private void RemoveOperation(Dictionary<ResourceType.TYPE, float> modDic, Dictionary<ResourceType.TYPE, float> addDic, 
                               ResourceType.TYPE resourceType, float modifier)
    {
        switch (m_operationType)
        {
            case EffectOperationType.TYPE.Multiply:
                if (modDic.ContainsKey(resourceType))
                {
                    modDic[resourceType] /= modifier;
                }
                break;
            case EffectOperationType.TYPE.Add:
                if (addDic.ContainsKey(resourceType))
                {
                    addDic[resourceType] -= modifier;
                }
                break;
        }
    }

    /// <summary>
    /// 이펙트 정보를 사용자에게 표시할 수 있는 문자열 반환 (오버라이드)
    /// </summary>
    /// <returns>이펙트 정보 문자열</returns>
    public override string GetEffectInfo()
    {
        string baseInfo = base.GetEffectInfo();
        
        if (!IsActive)
        {
            return baseInfo;
        }
        
        string targetInfo = m_targetType switch
        {
            EffectTargetType.TYPE.Building => "건물",
            EffectTargetType.TYPE.Territory => "영지",
            EffectTargetType.TYPE.Both => "건물 + 영지",
            _ => "알 수 없음"
        };
        
        string operationInfo = m_operationType switch
        {
            EffectOperationType.TYPE.Multiply => $"곱연산 ({m_modifier:F1}배)",
            EffectOperationType.TYPE.Add => $"합연산 (+{m_modifier:F1})",
            _ => "알 수 없음"
        };
        
        string scopeInfo = m_scopeType switch
        {
            EffectScopeType.TYPE.All => "모든 리소스",
            EffectScopeType.TYPE.Specific => $"{m_resourceType} 리소스",
            _ => "알 수 없음"
        };
        
        return $"{baseInfo}\n대상: {targetInfo}\n범위: {scopeInfo}\n연산: {operationInfo}";
    }
} 