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
            EditorUtility.DisplayDialog("Sync Units", "FactionData ������ ������ �� �����ϼ���.", "OK");
            return;
        }
        SyncUnitsForFaction(obj);
    }

    private static void SyncUnitsForFaction(FactionData faction)
    {
        // ������Ʈ ��ü���� UnitData Ž��
        string[] guids = AssetDatabase.FindAssets("t:UnitData");
        var list = new List<UnitData>(64);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var unit = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            if (!unit) continue;

            // ���� �ѼǸ� ����
            if (unit.factionType == faction.m_factionType)
                list.Add(unit);
        }

        // �ߺ� ����
        var set = new HashSet<UnitData>(list);
        list.Clear();
        list.AddRange(set);

        Undo.RecordObject(faction, "Sync Units");
        faction.m_units = list;
        EditorUtility.SetDirty(faction);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SyncFactionUnits] '{faction.m_name}'�� {list.Count}�� UnitData ����ȭ �Ϸ�");
    }
}
#endif
