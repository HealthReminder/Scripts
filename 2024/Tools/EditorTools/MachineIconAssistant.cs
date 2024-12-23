# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Post Processor will run after assets are imported
/// </summary>
class MachineIconPostProcessor : AssetPostprocessor
{
    public static TextureImporterPlatformSettings GetPlatformSettings()
    {
        var settings = new TextureImporterPlatformSettings();
        settings.name = "iPhone";
        settings.maxTextureSize = 2048;
        settings.resizeAlgorithm = 0;
        settings.textureCompression = TextureImporterCompression.Compressed;
        settings.format = TextureImporterFormat.ASTC_6x6;
        settings.compressionQuality = 50;
        settings.crunchedCompression = false;
        settings.allowsAlphaSplitting = false;
        settings.overridden = true;
        settings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        return settings;
    }

    // The main chunk of code will only run after we import machine icons
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (importedAssets == null || importedAssets.Length == 0 || MachineAssetPair.ImportMachineList == null || MachineAssetPair.ImportMachineList.Count() == 0) return;

        var failedImports = new List<MachineAssetPair>();

        foreach (MachineAssetPair machine in MachineAssetPair.ImportMachineList) 
        {
            var machineAssetBundleName = machine.AssetBundleName;
            if (string.IsNullOrEmpty(machineAssetBundleName))
            {
                machineAssetBundleName = "art/machineicons/" + machine.Title;
            }

            foreach (FileInfo file in machine.gameAsset.Assets.Values) 
            {
                if (!file.Exists) continue;

                var path = "Assets" + file.FullName.Substring(Application.dataPath.Length);
                var assetImporter = AssetImporter.GetAtPath(path);
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = machineAssetBundleName;
                    // Modify texture settings
                    TextureImporter textureImporter = (TextureImporter)assetImporter;
                    textureImporter.SetPlatformTextureSettings(GetPlatformSettings());
                }
                else
                {
                    failedImports.Add(machine);
                    break;
                }
            }
        }

        // Clean up failed machines
        foreach (MachineAssetPair machine in failedImports) 
        {
            Debug.LogError("Error importing " + machine.Title + "\nPlease attempt manually.");
            foreach (FileInfo file in machine.gameAsset.Assets.Values)
            {
                if (file.Exists)
                {
                    file.Delete();
                }
            }
            MachineAssetPair.ImportMachineList.Remove(machine); // Remove failed machine from import list
        }

        // Create Sprite Atlases for Casino
        if (EditorToolConfig.IsCasino())
        {
            var spriteAtlases = new List<SpriteAtlas>();
            foreach (MachineAssetPair machine in MachineAssetPair.ImportMachineList)
            {
                spriteAtlases.Add(CreateSpriteAtlas(machine));
            }
            SpriteAtlasUtility.PackAtlases(spriteAtlases.ToArray(), BuildTarget.NoTarget);
        }
        else
        {
            // Pack the the three large sprite atlases for Arcade
            if (MachineAssetPair.ImportMachineList.Count() > 0) 
            {
                var path = "Assets" + MachineAssetPair.ImportMachineList[0].gameAsset.RootDirectory.Substring(Application.dataPath.Length);

                var spriteAtlases = new SpriteAtlas[3]
                {
                    AssetDatabase.LoadAssetAtPath(Path.Combine(path, "LimitedTime.spriteatlas"), typeof(SpriteAtlas)) as SpriteAtlas,
                    AssetDatabase.LoadAssetAtPath(Path.Combine(path, "Square.spriteatlas"), typeof(SpriteAtlas)) as SpriteAtlas,
                    AssetDatabase.LoadAssetAtPath(Path.Combine(path, "Tall.spriteatlas"), typeof(SpriteAtlas)) as SpriteAtlas
                };

                SpriteAtlasUtility.PackAtlases(spriteAtlases, BuildTarget.NoTarget);
            }
        }
        MachineAssetPair.ImportMachineList.Clear();
    }

    public static SpriteAtlas CreateSpriteAtlas(MachineAssetPair machine)
    {
        SpriteAtlas sa = new SpriteAtlas();
        SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings();
        packingSettings.enableRotation = false;
        packingSettings.enableTightPacking = false;
        packingSettings.padding = 8;
        sa.SetPackingSettings(packingSettings);
        sa.SetPlatformSettings(GetPlatformSettings());

        var rootPath = "Assets" + machine.gameAsset.RootDirectory.Substring(Application.dataPath.Length);
        AssetDatabase.CreateAsset(sa, Path.Combine(rootPath, "atlas", machine.Title + ".spriteatlas"));
        foreach (FileInfo file in machine.gameAsset.Assets.Values)
        {
            if (!file.Exists) continue;
            var path = "Assets" + file.FullName.Substring(Application.dataPath.Length);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
            if (obj != null)
                SpriteAtlasExtensions.Add(sa, new UnityEngine.Object[] { obj });
        }
        AssetDatabase.SaveAssets();
        return sa;
    }
}

# region AssetData Classes
public abstract class AssetData
{
    public Dictionary<string, FileInfo> Assets;

    public string RootDirectory { get;  protected set; }

    public bool Exists
    {
        get { return Assets.Values.All(x => x.Exists); }
    }

    public int Count
    {
        get { return Assets.Count; }
    }

    public int CountExisting
    {
        get { return Assets.Values.Where(x => x.Exists).Count(); }
    }

    public string GetLogoPath()
    {
        FileInfo file = null;
        if (Assets.TryGetValue("Logo Square", out file) && file.Exists)
        {
            return file.FullName;
        }
        return null;
    }
}

public class ArcadeGameAssetData : AssetData
{
    public ArcadeGameAssetData(string title, string rootPath)
    {
        RootDirectory = rootPath;
        Assets = new Dictionary<string, FileInfo>();
        Assets.Add("Button Tall LT", new FileInfo(Path.Combine(rootPath, "LimitedTime", title + "_button_tall_lt.png")));
        Assets.Add("Logo Tall LT", new FileInfo(Path.Combine(rootPath, "LimitedTime", title + "_logo_tall_lt.png")));
        Assets.Add("Button Square", new FileInfo(Path.Combine(rootPath, "Square", title + "_button_square.png")));
        Assets.Add("Logo Square", new FileInfo(Path.Combine(rootPath, "Square", title + "_logo_square.png")));
        Assets.Add("Button Tall", new FileInfo(Path.Combine(rootPath, "Tall", title + "_button_tall.png")));
        Assets.Add("Logo Tall", new FileInfo(Path.Combine(rootPath, "Tall", title + "_logo_tall.png")));
    }
}

public class ArcadeArtAssetData : AssetData
{
    public ArcadeArtAssetData(string title, string rootPath)
    {
        RootDirectory = rootPath;
        Assets = new Dictionary<string, FileInfo>();
        Assets.Add("Button Tall LT", new FileInfo(Path.Combine(rootPath, title + "_button_tall_lt.png")));
        Assets.Add("Logo Tall LT", new FileInfo(Path.Combine(rootPath, title + "_logo_tall_lt.png")));
        Assets.Add("Button Square", new FileInfo(Path.Combine(rootPath, title + "_button_square.png")));
        Assets.Add("Logo Square", new FileInfo(Path.Combine(rootPath, title + "_logo_square.png")));
        Assets.Add("Button Tall", new FileInfo(Path.Combine(rootPath, title + "_button_tall.png")));
        Assets.Add("Logo Tall", new FileInfo(Path.Combine(rootPath, title + "_logo_tall.png")));
    }
}

public class CasinoGameAssetData : AssetData
{
    public CasinoGameAssetData(string title, string rootPath)
    {
        RootDirectory = rootPath;
        Assets = new Dictionary<string, FileInfo>();
        Assets.Add("Tile", new FileInfo(Path.Combine(rootPath, "gameTiles", title + "_tile.png")));
        Assets.Add("Button Square", new FileInfo(Path.Combine(rootPath, title + "_button_square.png")));
        Assets.Add("Button Tall", new FileInfo(Path.Combine(rootPath, title + "_button_tall.png")));
        Assets.Add("Logo Square", new FileInfo(Path.Combine(rootPath, title + "_logo_square.png")));
        Assets.Add("Logo Tall", new FileInfo(Path.Combine(rootPath, title + "_logo_tall.png")));
    }
}

public class CasinoArtAssetData : AssetData
{
    public CasinoArtAssetData(string title, string rootPath)
    {
        RootDirectory = rootPath;
        Assets = new Dictionary<string, FileInfo>();
        Assets.Add("Tile", new FileInfo(Path.Combine(rootPath, title + "_tile.png")));
        Assets.Add("Button Square", new FileInfo(Path.Combine(rootPath, title + "_button_square.png")));
        Assets.Add("Button Tall", new FileInfo(Path.Combine(rootPath, title + "_button_tall.png")));
        Assets.Add("Logo Square", new FileInfo(Path.Combine(rootPath, title + "_logo_square.png")));
        Assets.Add("Logo Tall", new FileInfo(Path.Combine(rootPath, title + "_logo_tall.png")));
    }
}
# endregion

public class MachineAssetPair
{
    public static List<MachineAssetPair> ImportMachineList;

    public bool IsSelected = true;

    [Flags]
    public enum SyncStatus
    {
        Synced = 1,
        NoGameAssets = 2,
        NoArtAssets = 4,
        PartialMissingGameAssets = 8,
        PartialMissingArtAssets = 16,
        Unknown = 32,
    }

    public AssetData gameAsset;
    public AssetData artAsset;

    public int Count { get { return artAsset.Count; } }

    public string Title;
    public Texture2D Logo { get; }

    public bool CanImport { get { return gameAsset.CountExisting == 0 || artAsset.CountExisting > gameAsset.CountExisting;  } }

    public bool IsSynced = false;
    public string AssetBundleName;

    public (SyncStatus status, string Message) GetSyncStatus
    {
        get
        {
            if (IsSynced)
                return (SyncStatus.Synced, "Synced");
            if (gameAsset.CountExisting == 0)
                return (SyncStatus.NoGameAssets, "No Game Assets");
            if (artAsset.CountExisting == 0)
                return (SyncStatus.NoArtAssets, "No Art Assets");
            if (artAsset.CountExisting > gameAsset.CountExisting)
                return (SyncStatus.PartialMissingGameAssets, "Missing some game assets");
            if (artAsset.CountExisting < gameAsset.CountExisting)
                return (SyncStatus.PartialMissingArtAssets, "Missing some art assets");
            return (SyncStatus.Unknown, "Unknown Status");
        }
    }

    public MachineAssetPair(string title, string artRootPath, string gameRootPath)
    {
        Title = title;
        artAsset = EditorToolConfig.IsCasino() ? new CasinoArtAssetData(title, artRootPath) : new ArcadeArtAssetData(title, artRootPath);
        gameAsset = EditorToolConfig.IsCasino() ? new CasinoGameAssetData(title, gameRootPath) : new ArcadeGameAssetData(title, gameRootPath);

        Logo = new Texture2D(1, 1);

        var logoPath = artAsset.GetLogoPath() ?? gameAsset.GetLogoPath(); // if logo from art folder is null, see if there is one in the unity project
        if (logoPath != null)
        {
            var rawData = System.IO.File.ReadAllBytes(logoPath);
            Logo.LoadImage(rawData);
        }

        if (ImportMachineList == null)
        {
            ImportMachineList = new List<MachineAssetPair>();
        }

        UpdateDetails();
    }

    public void UpdateDetails() 
    {
        IsSynced = GetSyncedAssets().Count == Count;
        AssetBundleName = GetAssetBundleName();
    }

    private string GetAssetBundleName()
    {
        foreach (FileInfo asset in gameAsset.Assets.Values)
        {
            if (!asset.Exists)
                continue;
            var relativePath = asset.FullName.Substring(asset.FullName.IndexOf("Assets"));
            var assetImporter = AssetImporter.GetAtPath(relativePath);
            if (assetImporter != null && !String.IsNullOrEmpty(assetImporter.assetBundleName))
            {
                return assetImporter.assetBundleName;
            }
        }
        return null;
    }

    public List<string> GetSyncedAssets()
    {
        var syncedTypeList = new List<string>();

        foreach (var gameAsset in gameAsset.Assets) 
        {
            bool match = gameAsset.Value.Exists == artAsset.Assets[gameAsset.Key].Exists;
            if (!match)
            {
                var message = gameAsset.Value.Exists ? "Art Asset Missing " : "Game Asset Missing";
            }
            else
            {
                syncedTypeList.Add(gameAsset.Key);
            }
        }
        return syncedTypeList;
    }

    public void ImportAssets()
    {
        int machineIndex = ImportMachineList.Count();
        for (int i = 0; i < Count; i++)
        {
            var artFile = artAsset.Assets.Values.ElementAt(i);
            var gameFile = gameAsset.Assets.Values.ElementAt(i);
            if (artFile.Exists)
            {
                var key = gameAsset.Assets.Keys.ElementAt(i);
                gameAsset.Assets[key] = artFile.CopyTo(gameFile.FullName, true);
            }
        }
        ImportMachineList.Add(this);
        UpdateDetails();
    }

    public void ReplaceAssets()
    {
        for (int i = 0; i < Count; i++)
        {
            var artFile = artAsset.Assets.Values.ElementAt(i);
            var gameFile = gameAsset.Assets.Values.ElementAt(i);
            if (artFile.Exists && gameFile.Exists)
            {
                var key = gameAsset.Assets.Keys.ElementAt(i);
                gameAsset.Assets[key] = artFile.CopyTo(gameFile.FullName, true);
            }
        }
        UpdateDetails();
    }

    public static GUIContent GetStatusIcon(SyncStatus status)
    {
        switch (status)
        {
            case SyncStatus.Synced:
                return EditorGUIUtility.IconContent("Installed@2x");
            case SyncStatus.NoGameAssets:
                return EditorGUIUtility.IconContent("Download-Available@2x");
            case SyncStatus.NoArtAssets:
                return EditorGUIUtility.IconContent("Error@2x");
            case SyncStatus.PartialMissingGameAssets:
                return EditorGUIUtility.IconContent("Warning@2x");
            case SyncStatus.PartialMissingArtAssets:
                return EditorGUIUtility.IconContent("Warning@2x");
            default:
                return EditorGUIUtility.IconContent("Error@2x");
        }
    }
}

public class MachineIconAssistant : EditorWindow
{
    public enum Page 
    {
        Main = 0,
        Operations,
        Details    
    }

    private Page currentPage = Page.Main;

    private static IEnumerable<MachineAssetPair> machineList = new List<MachineAssetPair>();
    private static IEnumerable<MachineAssetPair> filteredMachineList = new List<MachineAssetPair>();
    private static IEnumerable<MachineAssetPair> operationsPageList = new List<MachineAssetPair>();

    private string gamePath;
    private string artPath;
    private Vector2 scrollPosition;
    private Vector2 operationsScrollPosition;

    private bool dirtyBit = false;
    private bool searchFilterFoldout = false;
    private string searchField = "";
    private bool showMissingAssetBundle = false;
    private MachineAssetPair.SyncStatus statusFilter = (MachineAssetPair.SyncStatus)~0;
    private MachineAssetPair showMachineDetails;


    [MenuItem(EditorToolConfig.WINDOW_BLASTWORKS_TOOLS_PATH + "Machine Icon Assistant")]
    static void Init()
    {
        var window = GetWindow<MachineIconAssistant>("Machine Icon Assistant");
        window.position = new Rect(0, 0, 560, 800);
        window.Show();
    }

    private void OnBecameVisible()
    {
        artPath = PlayerPrefs.GetString(EditorToolConfig.Game.ToString() + "_icon_path_art");
        gamePath = PlayerPrefs.GetString(EditorToolConfig.Game.ToString() + "_icon_path_game");

        if (Directory.Exists(artPath) && Directory.Exists(gamePath))
        {
            FetchMachineAssets();
        }
    }

    void OnGUI()
    {
        switch (currentPage) 
        {
            case Page.Main:
                MainPage();
                break;
            case Page.Operations:
                AllOperationsPage();
                break;
            case Page.Details:
                ShowDetailsPage();
                break;
        }
    }

    private void MainPage() 
    {
        EditorGUILayout.BeginHorizontal();

        var newArtPath = EditorGUILayout.TextField("Art Assets Path", artPath);

        if (GUILayout.Button("...", GUILayout.Width(38)))
        {
            OnDirectorySelectBtnPress(true);
        }
        EditorGUILayout.EndHorizontal();

        if (!Directory.Exists(artPath))
        {
            EditorGUILayout.HelpBox("'Art Assets Path' is the path to your sar-lobby-images/images folder.", MessageType.Info);
        }
        // Game Path
        EditorGUILayout.BeginHorizontal();

        var newGamePath = EditorGUILayout.TextField("Game Assets Path", gamePath);

        if (GUILayout.Button("...", GUILayout.Width(38)))
        {
            OnDirectorySelectBtnPress(false);
        }

        EditorGUILayout.EndHorizontal();

        if (!Directory.Exists(gamePath))
        {
            EditorGUILayout.HelpBox("'Game Assets Path' is the path to the AssetBundles/MachineIcons folder within your Unity Project.", MessageType.Info);
        }

        // Return if there is nothing to show, until we have machine icons to show
        if (machineList.Count() == 0)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Import All..."))
        {
            operationsPageList = machineList.Where(x => x.CanImport);
            currentPage = Page.Operations;

        }
        if (GUILayout.Button("Replace All..."))
        {
            operationsPageList = machineList.Where(x => x.IsSynced);
            currentPage = Page.Operations;
        }

        if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(38)))
        {
            FetchMachineAssets();
        }
        EditorGUILayout.EndHorizontal();


        searchFilterFoldout = EditorGUILayout.Foldout(searchFilterFoldout, "Filters");

        if (searchFilterFoldout)
        {
            var searchFieldChange = EditorGUILayout.TextField("Search:", searchField);
            EditorGUILayout.BeginHorizontal();
            var showMissingAssetBundleChange = EditorGUILayout.Toggle("Missing AssetBundles", showMissingAssetBundle);
            var statusFilterChange = (MachineAssetPair.SyncStatus)EditorGUILayout.EnumFlagsField(statusFilter);

            if (statusFilterChange != statusFilter || showMissingAssetBundleChange != showMissingAssetBundle || !searchField.Equals(searchFieldChange))
            {
                dirtyBit = true;
                statusFilter = statusFilterChange;
                showMissingAssetBundle = showMissingAssetBundleChange;
                searchField = searchFieldChange;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (dirtyBit)
        {
            UpdateListByFilter();
        }

        DisplayMachineList();
    }

    private void UpdateListByFilter() 
    {
        if (searchField.Length >= 3)
        {
            try
            {
                Regex r = new Regex(searchField, RegexOptions.IgnoreCase);
                filteredMachineList = machineList.Where(x => r.IsMatch(x.Title));
            }
            catch (ArgumentException e)
            {

            }
        }
        else
        {
            filteredMachineList = machineList.Where(x => statusFilter.HasFlag(x.GetSyncStatus.status)
                && !(showMissingAssetBundle && !(x.AssetBundleName == null && x.GetSyncStatus.status != MachineAssetPair.SyncStatus.NoGameAssets)));
        }
        dirtyBit = false;
    }

    private void OnDirectorySelectBtnPress(bool isArtFolder)
    {
        string path = EditorUtility.OpenFolderPanel("Select Asset Folder", "", "");
        if (!string.IsNullOrEmpty(path))
        {
            var key = EditorToolConfig.Game.ToString() + "_icon_path" + (isArtFolder ? "_art" : "_game");
            PlayerPrefs.SetString(key, path);
            if (isArtFolder)
                artPath = path;
            else
                gamePath = path;

            if (Directory.Exists(artPath) && Directory.Exists(gamePath))
            {
                FetchMachineAssets();
            }
        }
    }

    private void DisplayMachineList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        int index = 0;
        foreach (var machine in filteredMachineList)
        {
            DisplayMachineListItem(machine, index++);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DisplayMachineListItem(MachineAssetPair machine, int index)
    {
        EditorGUILayout.BeginHorizontal(BackgroundStyle.GetListBackground(index));

        // Logo
        EditorGUILayout.LabelField(new GUIContent(machine.Logo, machine.Title), GUILayout.Width(116), GUILayout.Height(64));

        // Details Labels/Text
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_Texture Icon"), GUILayout.Width(24), GUILayout.Height(24));
        EditorGUILayout.LabelField(machine.Title, EditorStyles.boldLabel, GUILayout.Height(24));
        EditorGUILayout.EndHorizontal();

        var syncStatus = machine.GetSyncStatus;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(MachineAssetPair.GetStatusIcon(syncStatus.status), GUILayout.Width(24), GUILayout.Height(24));
        EditorGUILayout.LabelField(syncStatus.Message, GUILayout.Height(24));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (syncStatus.status == MachineAssetPair.SyncStatus.NoGameAssets)
        {
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_PrefabModel Icon"), GUILayout.Width(24), GUILayout.Height(24));
            EditorGUILayout.LabelField("art/machineicons/"+machine.Title, GUILayout.Height(24));
        } 
        else 
        {
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(machine.AssetBundleName != null ? "d_PrefabModel Icon" : "Warning@2x"), GUILayout.Width(24), GUILayout.Height(24));
            EditorGUILayout.LabelField(machine.AssetBundleName ?? "No AssetBundle name", GUILayout.Height(24));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // Buttons
        EditorGUILayout.BeginVertical();
        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(EditorGUIUtility.IconContent("_Menu"), GUILayout.Width(24), GUILayout.Height(24)))
        {
            showMachineDetails = machine;
            currentPage = Page.Details;
        }

        if ((syncStatus.status == MachineAssetPair.SyncStatus.NoGameAssets || syncStatus.status == MachineAssetPair.SyncStatus.PartialMissingGameAssets)
            && GUILayout.Button("Import", GUILayout.Width(70), GUILayout.Height(24)))
        {
            machine.ImportAssets();
            RefreshUI(true);
        }
        if (syncStatus.status == MachineAssetPair.SyncStatus.Synced && GUILayout.Button("Replace", GUILayout.Width(70), GUILayout.Height(24)))
        {
            machine.ReplaceAssets();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    public void ShowDetailsPage() 
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("< Back", GUILayout.Width(70), GUILayout.Height(30)))
        {
            showMachineDetails = null;
            currentPage = Page.Main;
            EditorGUILayout.EndHorizontal();
            return;
        }
        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.fontSize = 24;
        centeredStyle.fixedHeight = 30;

        EditorGUILayout.LabelField(showMachineDetails.Title, centeredStyle);
        EditorGUILayout.EndHorizontal();

        var boldStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        boldStyle.fontStyle = FontStyle.Bold;

        var showInFinderContent = EditorGUIUtility.IconContent("Folder Icon");
        showInFinderContent.tooltip = "Show in Explorer";

        int index = 0;
        EditorGUILayout.LabelField("Art Assets", boldStyle, GUILayout.Height(30));
        foreach (var keyValue in showMachineDetails.artAsset.Assets)
        {
            EditorGUILayout.BeginHorizontal(BackgroundStyle.GetListBackground(index++));
            EditorGUILayout.LabelField(keyValue.Key, GUILayout.Width(100));
            EditorGUILayout.LabelField(keyValue.Value.FullName.Split("\\images\\")[1]);
            if (keyValue.Value.Exists && GUILayout.Button(showInFinderContent, GUILayout.Width(24), GUILayout.Height(24)))
            {
                EditorUtility.RevealInFinder(keyValue.Value.FullName);
            }
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(keyValue.Value.Exists ? "Installed@2x" : "redlight"), GUILayout.Width(24), GUILayout.Height(24));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
        index = 0;
        EditorGUILayout.LabelField("Game Assets", boldStyle, GUILayout.Height(30));
        foreach (var keyValue in showMachineDetails.gameAsset.Assets)
        {
            EditorGUILayout.BeginHorizontal(BackgroundStyle.GetListBackground(index++));
            EditorGUILayout.LabelField(keyValue.Key, GUILayout.Width(100));
            EditorGUILayout.LabelField(keyValue.Value.FullName.Split("\\Assets\\")[1]);
            if (keyValue.Value.Exists && GUILayout.Button(showInFinderContent, GUILayout.Width(24), GUILayout.Height(24))) 
            {
                EditorUtility.RevealInFinder(keyValue.Value.FullName);
            }
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(keyValue.Value.Exists ? "Installed@2x" : "redlight"), GUILayout.Width(24), GUILayout.Height(24));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_PrefabModel Icon"), GUILayout.Width(24), GUILayout.Height(24));
        var assetBundleName = showMachineDetails.AssetBundleName;
        if (assetBundleName == null)
        {
            assetBundleName = "art/machineicons/" + showMachineDetails.Title + " (Not yet created)";
        }
        EditorGUILayout.LabelField(assetBundleName, GUILayout.Height(24));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(new GUIContent(showMachineDetails.Logo, showMachineDetails.Title), GUILayout.Height(200));
    }

    public void AllOperationsPage()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("< Back", GUILayout.Width(70), GUILayout.Height(30)) || operationsPageList == null || operationsPageList.Count() == 0)
        {
            currentPage = Page.Main;
            EditorGUILayout.EndHorizontal();
            return;
        }

        bool isImport = !operationsPageList.FirstOrDefault().IsSynced;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        bool selectAll = operationsPageList.All(x => x.IsSelected);
        bool onSelectAllChange = false;
        var toggle = EditorGUILayout.Toggle(selectAll, GUILayout.Width(32));
        onSelectAllChange = toggle != selectAll;

        var selectAllStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        selectAllStyle.fontStyle = FontStyle.Bold;
        selectAllStyle.alignment = TextAnchor.UpperLeft;
        EditorGUILayout.LabelField("Select All", selectAllStyle);
        if (GUILayout.Button(isImport ? "Import" : "Replace"))
        {
            Debug.ClearDeveloperConsole(); // Clear the console before we start so it's easier to see any errors
            MachineAssetPair.ImportMachineList?.Clear();
            foreach (MachineAssetPair asset in operationsPageList)
            {
                if (asset.IsSelected)
                {
                    if (isImport)
                        asset.ImportAssets();
                    else
                        asset.ReplaceAssets();
                }
            }
            RefreshUI(isImport);
        }

        // Button to generate solo atlases for machines in Arcade
        if (!isImport && GUILayout.Button("Gen Solo Atlases")) 
        {
            var atlasFolder = Path.Combine(gamePath, "atlas");
            if (!Directory.Exists(atlasFolder))
            {
                Directory.CreateDirectory(atlasFolder);
            }
            
            var spriteAtlases = new List<SpriteAtlas>();
            foreach (MachineAssetPair machine in operationsPageList)
            {
                if (machine.IsSelected)
                {
                    spriteAtlases.Add(MachineIconPostProcessor.CreateSpriteAtlas(machine));
                }
            }
            SpriteAtlasUtility.PackAtlases(spriteAtlases.ToArray(), BuildTarget.NoTarget);
        }

        EditorGUILayout.EndHorizontal();

        operationsScrollPosition = EditorGUILayout.BeginScrollView(operationsScrollPosition);
        int index = 0;
        foreach (MachineAssetPair asset in operationsPageList)
        {
            EditorGUILayout.BeginHorizontal(BackgroundStyle.GetListBackground(index++));
            if (onSelectAllChange) 
            {
                asset.IsSelected = toggle;
            }

            asset.IsSelected = EditorGUILayout.Toggle(asset.IsSelected, GUILayout.Width(32));
            EditorGUILayout.LabelField(asset.Title);

            var status = asset.GetSyncStatus;
            EditorGUILayout.LabelField(MachineAssetPair.GetStatusIcon(status.status), GUILayout.Width(18), GUILayout.Height(18));
            EditorGUILayout.LabelField(status.Message);


            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }


    public void FetchMachineAssets()
    {
        var fetchedMachineList = new List<MachineAssetPair>();
        var machineNames = new HashSet<string>();
        
        // Art Path 
        var artDir = new DirectoryInfo(artPath);
        var gameDir = new DirectoryInfo(gamePath);

        var artFiles = artDir.GetFiles("*.png").Select(x => x.Name.Split('_')[0]);
        var gameFiles = gameDir.GetFiles("*.png").Select(x => x.Name.Split('_')[0]);
        var fileNames = artFiles.Union(gameFiles);

        foreach (var key in fileNames)
        {
            fetchedMachineList.Add(new MachineAssetPair(key, artPath, gamePath));
        }
        machineList = fetchedMachineList.OrderBy(x => x.IsSynced ? 1 : 0);
        filteredMachineList = machineList.ToList();
    }

    public void RefreshUI(bool isImport) 
    {
        FetchMachineAssets();

        if(isImport)
            operationsPageList = machineList.Where(x => x.CanImport);
        else
            operationsPageList = machineList.Where(x => x.IsSynced);

        dirtyBit = true;
        AssetDatabase.Refresh();
    }
}
#endif
