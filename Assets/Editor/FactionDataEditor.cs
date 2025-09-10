using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(FactionData))]
public class FactionDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FactionData factionData = (FactionData)target;

        if (GUILayout.Button("Automatically applies research to the faction"))
        {
            ApplyFactionResearch(factionData);
        }
    }

    private void ApplyFactionResearch(FactionData factionData)
    {
        string[] guids = AssetDatabase.FindAssets("t:ResearchData");
        List<FactionResearchData> allResearch = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<FactionResearchData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(r => r != null)
            .ToList();

        // 현재 팩션 타입과 일치하거나 공통 연구(None)인 것만 필터링
        var filtered = allResearch
            .Where(r => r.m_factionType == factionData.m_factionType)
            .ToList();

        factionData.m_research = filtered;
        EditorUtility.SetDirty(factionData);

        Debug.Log($"{filtered.Count} research {factionData.m_factionType} applied to faction.");
    }
}
