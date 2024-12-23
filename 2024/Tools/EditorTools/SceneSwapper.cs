# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Unity Editor Tool for quick access scene switching (Boot scene, Lobby, and any prefab)
/// </summary>
public class SceneSwapper : EditorWindow
{
    private int currentPickerWindow;
    private List<GameObject> prefabList = new List<GameObject>();
    private Vector2 scrollValue = Vector2.zero;

    [MenuItem(EditorToolConfig.WINDOW_BLASTWORKS_TOOLS_PATH + "Scene Swapper")]
    public static void Init()
    {
        var window = GetWindow<SceneSwapper>("Scene Swapper");
        window.position = new Rect(0, 0, 300, 300);
        window.minSize = new Vector2(250, 24);
        window.Show();
    }

    void OnGUI()
    {
        // The actual window code goes here
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("BuildSettings.Editor"), GUILayout.Width(24), GUILayout.Height(24));
        EditorGUILayout.LabelField("Switch Scenes: ", EditorStyles.boldLabel, GUILayout.Width(100), GUILayout.Height(24));
        if (GUILayout.Button("Boot", GUILayout.Height(24)))
        {
            EditorSceneManager.OpenScene("Assets/Thanos/BootScene.unity", OpenSceneMode.Single);
        }
        else if (GUILayout.Button("Lobby", GUILayout.Height(24)))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel, GUILayout.Width(100), GUILayout.Height(24));

        scrollValue = EditorGUILayout.BeginScrollView(scrollValue);
        int index = 0;
        foreach (var prefab in prefabList.ToList())
        {
            DrawPrefabListItem(prefab, index++);
        }

        // Add Prefab
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Prefab", GUILayout.Height(24)))
        {
            ShowPicker();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        GameObject selectedPrefab = null;
        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
        {
            selectedPrefab = (GameObject)EditorGUIUtility.GetObjectPickerObject();
            currentPickerWindow = -1;

            //name of selected object from picker
            prefabList.Add(selectedPrefab);
        }

    }

    private void ShowPicker()
    {
        //create a window picker control ID
        currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
        //use the ID you just created
        EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", currentPickerWindow);
    }

    public void DrawPrefabListItem(GameObject prefab, int index) 
    {
        EditorGUILayout.BeginHorizontal(BackgroundStyle.GetListBackground(index));
        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Prefab Icon"), GUILayout.Width(24), GUILayout.Height(24));
        EditorGUILayout.LabelField(prefab.name, GUILayout.MinWidth(100), GUILayout.Height(24));
        if (GUILayout.Button("Open",GUILayout.MinWidth(50), GUILayout.Height(24)))
        {
            PrefabStageUtility.OpenPrefab(AssetDatabase.GetAssetPath(prefab));
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_winbtn_win_close@2x"), GUILayout.Width(24), GUILayout.Height(24)))
        {
            prefabList.Remove(prefab);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
