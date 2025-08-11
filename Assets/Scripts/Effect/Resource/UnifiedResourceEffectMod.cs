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

    public override void Activate(GameDataManager argDataManager)
    {
        if (m_scopeType == EffectScopeType.TYPE.All)
        {
            // 모든 리소스에 적용
            foreach (ResourceType.TYPE type in System.Enum.GetValues(typeof(ResourceType.TYPE)))
            {
                ApplyModifier(argDataManager, type);
            }
        }
        else
        {
            // 특정 리소스에만 적용
            ApplyModifier(argDataManager, m_resourceType);
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        if (m_scopeType == EffectScopeType.TYPE.All)
        {
            // 모든 리소스에서 제거
            foreach (ResourceType.TYPE type in System.Enum.GetValues(typeof(ResourceType.TYPE)))
            {
                RemoveModifier(argDataManager, type);
            }
        }
        else
        {
            // 특정 리소스에서만 제거
            RemoveModifier(argDataManager, m_resourceType);
        }
    }

    private void ApplyModifier(GameDataManager argDataManager, ResourceType.TYPE resourceType)
    {
        switch (m_targetType)
        {
            case EffectTargetType.TYPE.Building:
                ApplyOperation(argDataManager.EventEntry.m_state.m_buildingResourceModDic, 
                             argDataManager.EventEntry.m_state.m_buildingResourceAddDic, resourceType, m_modifier);
                break;
            case EffectTargetType.TYPE.Territory:
                ApplyOperation(argDataManager.EventEntry.m_state.m_territoryResourceModDic, 
                             argDataManager.EventEntry.m_state.m_territoryResourceAddDic, resourceType, m_modifier);
                break;
            case EffectTargetType.TYPE.Both:
                ApplyOperation(argDataManager.EventEntry.m_state.m_buildingResourceModDic, 
                             argDataManager.EventEntry.m_state.m_buildingResourceAddDic, resourceType, m_modifier);
                ApplyOperation(argDataManager.EventEntry.m_state.m_territoryResourceModDic, 
                             argDataManager.EventEntry.m_state.m_territoryResourceAddDic, resourceType, m_modifier);
                break;
        }
    }

    private void RemoveModifier(GameDataManager argDataManager, ResourceType.TYPE resourceType)
    {
        switch (m_targetType)
        {
            case EffectTargetType.TYPE.Building:
                RemoveOperation(argDataManager.EventEntry.m_state.m_buildingResourceModDic, 
                              argDataManager.EventEntry.m_state.m_buildingResourceAddDic, resourceType, m_modifier);
                break;
            case EffectTargetType.TYPE.Territory:
                RemoveOperation(argDataManager.EventEntry.m_state.m_territoryResourceModDic, 
                              argDataManager.EventEntry.m_state.m_territoryResourceAddDic, resourceType, m_modifier);
                break;
            case EffectTargetType.TYPE.Both:
                RemoveOperation(argDataManager.EventEntry.m_state.m_buildingResourceModDic, 
                              argDataManager.EventEntry.m_state.m_buildingResourceAddDic, resourceType, m_modifier);
                RemoveOperation(argDataManager.EventEntry.m_state.m_territoryResourceModDic, 
                              argDataManager.EventEntry.m_state.m_territoryResourceAddDic, resourceType, m_modifier);
                break;
        }
    }

    private void ApplyOperation(Dictionary<ResourceType.TYPE, float> modDic, Dictionary<ResourceType.TYPE, float> addDic, 
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
                break;
            case EffectOperationType.TYPE.Add:
                if (!addDic.ContainsKey(resourceType))
                {
                    addDic[resourceType] = 0.0f; // 기본값 설정
                }
                addDic[resourceType] += modifier;
                break;
        }
    }

    private void RemoveOperation(Dictionary<ResourceType.TYPE, float> modDic, Dictionary<ResourceType.TYPE, float> addDic, 
                               ResourceType.TYPE resourceType, float modifier)
    {
        switch (m_operationType)
        {
            case EffectOperationType.TYPE.Multiply:
                if (!modDic.ContainsKey(resourceType))
                {
                    return; // 제거할 값이 없음
                }
                modDic[resourceType] /= modifier;
                break;
            case EffectOperationType.TYPE.Add:
                if (!addDic.ContainsKey(resourceType))
                {
                    return; // 제거할 값이 없음
                }
                addDic[resourceType] -= modifier;
                break;
        }
    }
} 