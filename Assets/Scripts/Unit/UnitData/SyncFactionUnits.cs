#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SyncFactionUnits
{
    [MenuItem("Tools/Faction/Sync Units From Project (Selection)")]
    public static void SyncSelectedFaction()
    {
        var obj = Selection.activeObject as FactionData;
        if (obj == null)
        {
            EditorUtility.DisplayDialog("Sync Units", "FactionData 에셋을 선택한 뒤 실행하세요.", "OK");
            return;
        }
        SyncUnitsForFaction(obj);
    }

    private static void SyncUnitsForFaction(FactionData faction)
    {
        // 프로젝트 전체에서 UnitData 탐색
        string[] guids = AssetDatabase.FindAssets("t:UnitData");
        var list = new List<UnitData>(64);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var unit = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            if (!unit) continue;

            // 같은 팩션만 수집
            if (unit.factionType == faction.m_factionType)
                list.Add(unit);
        }

        // 중복 제거
        var set = new HashSet<UnitData>(list);
        list.Clear();
        list.AddRange(set);

        Undo.RecordObject(faction, "Sync Units");
        faction.m_units = list;
        EditorUtility.SetDirty(faction);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SyncFactionUnits] '{faction.m_name}'에 {list.Count}개 UnitData 동기화 완료");
    }
}
#endif
