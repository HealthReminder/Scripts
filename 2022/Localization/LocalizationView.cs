using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationView : MonoBehaviour
{
    public Dropdown dropDown;

    private void Start()
    {
        dropDown.value = PlayerPrefs.GetInt("Language");    
    }

    public void ChangeLanguage()
    {
        PlayerPrefs.SetInt("Language", dropDown.value);
    }
}
