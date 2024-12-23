#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.U2D;

/// <summary>
/// Execute this script through the Tools dropdown to:
/// Find and print paths of non-power-of-two textures or sprites in specified folders,
/// excluding textures in certain ignored subfolders, and keep the list sorted by file size.
/// </summary>
public class FindNonPowerOfTwoTexturesTool
{
    /// Define list of folders to scan
    static List<string> foldersToScan = new List<string>
    {
        "Assets/Art"
    };

    /// Define the list of subfolders to ignore
    static List<string> ignoredFolders = new List<string>
    {
        "Assets/Art/AssetBundles",
        "Assets/Art/AppIcons"
    };

    [MenuItem("Tools/Find Non-Power of Two Assets")]
    public static void FindNonPowerOfTwoTextures()
    {
        FindAssets("t:Texture2D", asset =>
        {
            Texture2D texture = asset as Texture2D;
            if (texture != null && (!IsPowerOfTwo(texture.width) || !IsPowerOfTwo(texture.height)))
            {
                bool inSpriteAtlas = IsInSpriteAtlas(texture);
                return !inSpriteAtlas;
            }
            return false;
        });
    }

    /* Sprites are part of Texture2D so no need to look for them separately
    [MenuItem("Tools/Find Non-Power of Two Assets/Sprites")]
    public static void FindNonPowerOfTwoSprites()
    {
        FindAssets("t:Texture2D", asset =>
        {
            Texture2D texture = asset as Texture2D;
            if (texture != null)
            {
                // Check associated sprites within the texture
                Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(texture));
                foreach (Object subAsset in subAssets)
                {
                    Sprite sprite = subAsset as Sprite;
                    if (sprite != null && (!IsPowerOfTwo((int)sprite.rect.width) || !IsPowerOfTwo((int)sprite.rect.height)))
                    {
                        return true;
                    }
                }
            }
            return false;
        });
    }
    */

    private static void FindAssets(string filter, System.Predicate<Object> criteria)
    {
        int count = 0;
        List<(string path, long sizeInBytes, string readableSize)> pathsBySize = new List<(string path, long sizeInBytes, string readableSize)>();

        foreach (string folderToScan in foldersToScan)
        {
            // Find all assets matching the filter in the current folder
            string[] assetGUIDs = AssetDatabase.FindAssets(filter, new[] { folderToScan });

            foreach (string guid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Check if the path starts with any ignored folder
                if (ShouldIgnore(path)) continue;

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                // Check the criteria for inclusion
                if (asset != null && criteria(asset))
                {
                    long fileSize = GetFileSize(path);
                    if (fileSize > 0)
                    {
                        string readableSize = FormatSize(fileSize);
                        InsertInOrder(pathsBySize, (path, fileSize, readableSize));
                        count++;
                    }
                }
            }
        }

        // Log sorted paths
        string s = "";
        foreach (var entry in pathsBySize)
        {
            s += $"Path: {entry.path}, Size: {entry.readableSize} \n";
        }
        Debug.Log($"Sorted list of assets matching criteria (Total: {count}): \n{s}");
    }

    private static bool ShouldIgnore(string path)
    {
        foreach (string ignoredFolder in ignoredFolders)
        {
            if (path.StartsWith(ignoredFolder))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsPowerOfTwo(int value)
    {
        return (value & (value - 1)) == 0 && value > 0;
    }

    /* There is a chance that the way we get file size is wrong and this might help
    private static long GetFileSize(string assetPath)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        if (File.Exists(fullPath))
        {
            return new FileInfo(fullPath).Length;
        }
        return -1; // Return -1 for inaccessible files to ensure they're sorted at the end
    }
    */
    private static long GetFileSize(string assetPath)
    {
        // Load the texture asset
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        // Check if the texture is valid
        if (texture != null)
        {
            // Get the raw texture data size in bytes
            try
            {
                return texture.GetRawTextureData().Length;
            }
            catch (UnityException e)
            {
                Debug.LogWarning($"Failed to get raw texture data for: {assetPath}. Error: {e.Message}");
                return -1; // Return -1 if raw texture data is not available
            }
        }

        // Fallback to checking the file size on disk if it's not a texture
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        if (File.Exists(fullPath))
        {
            return new FileInfo(fullPath).Length;
        }

        return -1; // Return -1 for inaccessible files
    }
    /// <summary>
    /// Check if a Texture2D is part of any Sprite Atlas.
    /// </summary>
    private static bool IsInSpriteAtlas(Texture2D texture)
    {
        if (texture == null) return false;

        // Get the asset path of the texture
        string assetPath = AssetDatabase.GetAssetPath(texture);

        // Load all Sprite sub-assets of the texture
        Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        foreach (Object subAsset in subAssets)
        {
            Sprite sprite = subAsset as Sprite;
            if (sprite != null)
            {
                // Load all Sprite Atlases in the project
                string[] spriteAtlasGUIDs = AssetDatabase.FindAssets("t:SpriteAtlas");
                foreach (string guid in spriteAtlasGUIDs)
                {
                    string spriteAtlasPath = AssetDatabase.GUIDToAssetPath(guid);
                    SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasPath);

                    if (spriteAtlas != null && spriteAtlas.CanBindTo(sprite))
                    {
                        return true; // Sprite is part of a sprite atlas
                    }
                }
            }
        }

        return false; // Texture is not part of any sprite atlas
    }
    private static string FormatSize(long sizeInBytes)
    {
        if (sizeInBytes >= 1_048_576) // 1 MB or larger
            return $"{(sizeInBytes / 1_048_576f):0.##} MB";
        if (sizeInBytes >= 1024) // 1 KB or larger
            return $"{(sizeInBytes / 1024f):0.##} KB";
        return $"{sizeInBytes} bytes"; // Smaller than 1 KB
    }

    private static void InsertInOrder(List<(string path, long sizeInBytes, string readableSize)> list, (string path, long sizeInBytes, string readableSize) item)
    {
        // Find the correct position to insert the new item
        int index = 0;
        while (index < list.Count && list[index].sizeInBytes > item.sizeInBytes)
        {
            index++;
        }

        // Insert the item at the correct position
        list.Insert(index, item);
    }
}
#endif
