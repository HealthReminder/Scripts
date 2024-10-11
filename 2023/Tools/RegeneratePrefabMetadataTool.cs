# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
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