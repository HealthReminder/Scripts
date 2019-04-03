using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//LOCALIZATION DATA
[System.Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;
}

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}


public class LocalizationController : MonoBehaviour
{

    public static LocalizationController instance;

    private Dictionary<string, string> localizedText;
    private bool isReady = false;
    private string missingTextString = "Localized text not found";

    //Singleton pattern
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LoadLocalizedText();
    }

    public void LoadLocalizedText()
    {
        string fileName = FromIntToFileName(PlayerPrefs.GetInt("Language"));
        Debug.Log(fileName);
        //This function gets the file containing the translations for all the texts
        localizedText = new Dictionary<string, string>();   
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

            for (int i = 0; i < loadedData.items.Length; i++)
            {
                localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }

            Debug.Log("Data loaded, dictionary contains: " + localizedText.Count + " entries");
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }

        isReady = true;
    }

    public string GetLocalizedValue(string key)
    {
        //This function will be called from each localized text components 
        //So that it can fin the proper text to display
        string result = missingTextString;
        if (localizedText.ContainsKey(key))
        {
            result = localizedText[key];
        }

        return result;

    }
    public bool GetIsReady()
    {
        return isReady;
    }

    public string FromIntToFileName(int languageIndex)
    {
        //This function will take an int (sent by the menu dropdown)
        //And get the proper file name for its language
        string fileName = string.Empty;
        if (languageIndex == 0)
            fileName = "localization_pt.json";
        else if ( languageIndex == 1)
            fileName = "localization_en.json";
        else if(languageIndex == 2)
            fileName = "localization_es.json";
        else
            fileName = "localization_en.json";

        return (fileName);
    }

    public void UpdateAllTextInScene()
    {
        //This coroutine will be called when the palyer updates the language
        //When the accepted button is pressed, the routine will get all
        //Localized text components in the scene and update them accordingly
        StartCoroutine(UpdateAllTextCoroutine());
    }
    private IEnumerator UpdateAllTextCoroutine()
    {
        while (!isReady)
            yield return null;

        LocalizedText[] allTextInScene = FindObjectsOfType<LocalizedText>();
        foreach (LocalizedText lT in allTextInScene)
            StartCoroutine(lT.LoadText());
        Debug.Log("Updated all " +allTextInScene.Length+ " text in scene to "+ PlayerPrefs.GetInt("Language"));
        yield break;
    }

}
