using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{

    public string key;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(LoadText());
    }

    public IEnumerator LoadText()
    {
        while (!LocalizationController.instance.GetIsReady())
            yield return null;
           
        Text text = GetComponent<Text>();
        string oldText = text.text;
        text.text = LocalizationController.instance.GetLocalizedValue(key);
        Debug.Log("Updated Text from "+oldText + " to "+ text.text);
    }

}