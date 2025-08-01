using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnifiedResourceEffectMod", menuName = "Event Effects/Resource/Unified Resource Mod")]
public class UnifiedResourceEffectMod : EffectBase
{
    [Header("Effect Configuration")]
    public EffectTargetType.TYPE m_targetType = EffectTargetType.TYPE.Building;
    public EffectScopeType.TYPE m_scopeType = EffectScopeType.TYPE.Specific;
    public ResourceType.TYPE m_resourceType = ResourceType.TYPE.Wood;
    public float m_modifier = 1.2f; // 배수 (1.2 = 20% 증가)

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
                argDataManager.EventEntry.m_state.m_buildingResourceModDic[resourceType] *= m_modifier;
                break;
            case EffectTargetType.TYPE.Territory:
                argDataManager.EventEntry.m_state.m_territoryResourceModDic[resourceType] *= m_modifier;
                break;
            case EffectTargetType.TYPE.Both:
                argDataManager.EventEntry.m_state.m_buildingResourceModDic[resourceType] *= m_modifier;
                argDataManager.EventEntry.m_state.m_territoryResourceModDic[resourceType] *= m_modifier;
                break;
        }
    }

    private void RemoveModifier(GameDataManager argDataManager, ResourceType.TYPE resourceType)
    {
        switch (m_targetType)
        {
            case EffectTargetType.TYPE.Building:
                argDataManager.EventEntry.m_state.m_buildingResourceModDic[resourceType] /= m_modifier;
                break;
            case EffectTargetType.TYPE.Territory:
                argDataManager.EventEntry.m_state.m_territoryResourceModDic[resourceType] /= m_modifier;
                break;
            case EffectTargetType.TYPE.Both:
                argDataManager.EventEntry.m_state.m_buildingResourceModDic[resourceType] /= m_modifier;
                argDataManager.EventEntry.m_state.m_territoryResourceModDic[resourceType] /= m_modifier;
                break;
        }
    }
} 