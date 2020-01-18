using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTextBehaviour : MonoBehaviour
{
    public Text text;
    public string[] availableTexts;
    int index = 0;

    public void ChangeText(string newText){
        if(availableTexts.Length <= 0)
            return;

        index++;
        if(index >= availableTexts.Length)
            index = 0; 
            
        text.text = availableTexts[index];               
    }
}
