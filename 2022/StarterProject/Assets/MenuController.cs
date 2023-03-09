using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuObject;
    //Make it a singleton
	[HideInInspector]    public static MenuController instance;
    void Awake(){
		//Singleton pattern
		if  (instance == null){
			DontDestroyOnLoad(gameObject);
			instance = this;
		}	
		else if (instance != this)
			Destroy(gameObject);
		
	}

    //This function is responsible for turning the main menu on or off
    public void ShowMainMenu (bool isActive) {
        if(isActive){
            mainMenuObject.SetActive(true);
        } else {
            mainMenuObject.SetActive(false);
        }
        
    }

}
