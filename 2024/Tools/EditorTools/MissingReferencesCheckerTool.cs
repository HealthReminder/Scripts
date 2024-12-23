#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Execute this script through the Tools dropdown to:
/// Count how many missing scripts are in your prefabs
/// Count how many prefabs are missing the references to their parent prefabs
/// * This tool does not do anything itself apart from counting the above *
/// </summary>
public class MissingReferencesCheckerTool
{
    [MenuItem("Tools/Count Missing References")]

    private static void CountMissingReferences()
    {
        int missingScriptCount = 0;
        int missingPrefabCount = 0;
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            // Check for missing scripts
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            int currentMissingScriptsCount = 0;
            int currentMissingPrefabsCount = 0;
            foreach (Component component in components)
            {
                if (component == null)
                {
                    if(currentMissingScriptsCount == 0)
                    {
                        missingScriptCount++;
                    }
                    currentMissingScriptsCount++;
                }
            }
            // Check for missing prefab references
            Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(child))
                {
                    if (PrefabUtility.GetCorrespondingObjectFromSource(child) == null) {
                        if (currentMissingPrefabsCount == 0)
                        {
                            missingPrefabCount++;
                        }
                        currentMissingPrefabsCount++;
                        break;
                    } 
                }
            }
            if (currentMissingScriptsCount > 0)
            {
                Debug.LogWarning($"Found {currentMissingScriptsCount} scripts missing from prefab: {path}");
            }
            if (currentMissingPrefabsCount > 0)
            {
                Debug.LogWarning($"Found {currentMissingPrefabsCount} prefabs missing from prefab: {path}");
            }
        }

        Debug.Log($"Total Prefabs with Missing Scripts: {missingScriptCount}");
        Debug.Log($"Total Prefabs with Missing Prefab References: {missingPrefabCount}");
    }
}
#endif