# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
/// <summary>
/// Execute this script through the Tools dropdown to:
/// Regenerate prefab metadata by automatically opening prefabs, marking them dirty and saving them
/// This should regenerate prefab metadata with no apparent modifications
/// * Use the checker tool for missing scripts and prefabs reference before this tool. Otherwise this will fail *
/// </summary>
public class RegeneratePrefabMetadataTool 
{
    [MenuItem("Tools/Regenerate Prefab Metadata")]
    public static void UpdatePrefabMetadata()
    {
        // Find all prefabs in the project
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                if (assetPath.StartsWith("Packages/"))
                {
                    Debug.Log($"Metadata update skipped for prefab in Packages folder: {prefab.name}");
                }
                else
                {
                    // Mark the prefab as dirty to force Unity to recognize changes and save
                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                    Debug.Log($"Metadata updated for prefab: {prefab.name}");
                    count++;
                }
            }
            else
            {
                Debug.LogError("Prefab not found at specified path.");
            }
        }
        Debug.Log($"Updated {count} prefab metadata.");
    }
}
#endif