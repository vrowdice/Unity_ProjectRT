using UnityEditor;
using UnityEngine;
using System.Linq; // Required for .Any()
using System; // Required for Enum.GetValues

[CustomEditor(typeof(GameBalanceData))]
public class GameBalanceDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields for the ScriptableObject.
        DrawDefaultInspector();

        // Get a reference to the target GameBalanceData asset.
        GameBalanceData gameBalanceData = (GameBalanceData)target;

        // --- Logic to ensure the list is initialized and kept up-to-date ---
        // This runs whenever the inspector is displayed.
        // It's designed to populate if empty, or add missing entries if the enum changes.
        bool needsInitialization = gameBalanceData.m_balanceTypeMulList == null ||
                                   gameBalanceData.m_balanceTypeMulList.Count == 0;

        if (!needsInitialization)
        {
            // Check if any BalanceType enum member is missing from the list.
            // This is especially useful if you add new ranks to your BalanceType enum later.
            bool hasMissingRanks = false;
            foreach (BalanceType rank in Enum.GetValues(typeof(BalanceType)))
            {
                if (!gameBalanceData.m_balanceTypeMulList.Any(item => item.m_royalRank == rank))
                {
                    hasMissingRanks = true;
                    break;
                }
            }

            if (hasMissingRanks)
            {
                needsInitialization = true; // Flag for re-initialization to add missing ones.
            }
        }

        if (needsInitialization)
        {
            gameBalanceData.InitializeBalanceTypeMulList();
            // Mark the ScriptableObject as dirty so Unity saves the changes to disk.
            EditorUtility.SetDirty(gameBalanceData);
        }

        // Optional: Add a button to manually re-initialize the list.
        // This is useful for debugging or if you want to force a reset.
        if (GUILayout.Button("Re-initialize Balance Type List"))
        {
            gameBalanceData.InitializeBalanceTypeMulList();
            EditorUtility.SetDirty(gameBalanceData); // Mark as dirty after manual re-initialization.
        }
    }

    // This method is called when the Inspector window for the ScriptableObject is enabled,
    // which happens when the asset is first selected or created.
    private void OnEnable()
    {
        GameBalanceData gameBalanceData = (GameBalanceData)target;
        if (gameBalanceData != null)
        {
            // Perform an initial setup when the asset is loaded into the Inspector.
            // This ensures the list is populated right after creation (via CreateAssetMenu).
            gameBalanceData.InitializeBalanceTypeMulList();
            EditorUtility.SetDirty(gameBalanceData); // Mark as dirty to save the initial state.
        }
    }
}