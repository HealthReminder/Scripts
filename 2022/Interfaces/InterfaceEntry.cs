using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterfaceEntry : MonoBehaviour
{

    //The GUI designer must use this class to designate field like
    //Name, price, power, things like that
    //Null values should just be ignored
    //This class represents the Object in the world
    //That displays the interface information to the player
    //Info
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public Image ValueImage;
    //Cosmetics
    public Image BackgroundImage;

    public void SetEntry(string name = "", string description = "")
    {
        string empty = string.Empty;

        if (name != empty)
            if(NameText)
            NameText.text = name;

        if (description != empty)
            if(DescriptionText)
                DescriptionText.text = description;
    }
}
