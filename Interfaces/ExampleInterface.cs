using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleInterface : Interface
{
    void Start()
    {
        SetupInterface();
    }
    private void Update()
    {
        //Interfaces can do two things: Add entries and clear entries
        //The difference is in the design of the GUI
        if (Input.GetKeyDown(KeyCode.R))
            ClearEntries();
        if (Input.GetKeyDown(KeyCode.A))
            AddEntry("TestObject", "Description");
    }
    public override void SetupInterface()
    {
        base.SetupInterface();

    }
}
