using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pooling;
using UnityEditor.Profiling.Memory.Experimental;

public abstract class InterfaceProperties : MonoBehaviour
{
    [SerializeField] public InterfaceEntry EntryTemplate;
    public Transform entryContainer;
    public int PoolSize = 20;
}
public abstract class InterfaceData : InterfaceProperties
{
    //This class contains the current actual data 
    //Being displayed by this class
    [SerializeField]public Pool Pool;
    public List<int> displayingIds;
}
public abstract class InterfaceController : InterfaceData
{
    //This class is responsible for the inner workings of the interface
    //It has conversion and instantiation classes
}

public abstract class Interface : InterfaceController
{
    //This class is common to all GUI classes
    //It contains the basics for displaying information
    //Like an "EntryTemplate" for example
    //It can represent a single button, single text
    //It can represent complex arrangements of text and buttons
    //It can represent a list of the mentioned above
    public virtual void SetupInterface()
    {
        displayingIds = new List<int>();
        Pool = new Pool(PoolSize, EntryTemplate.gameObject, entryContainer);
    }
    public void AddEntry(string name, string description)
    {
        int newID = Pool.ReserveObject();
        displayingIds.Add(newID);

        InterfaceEntry entry = Pool.GetObjectFromID(newID).GetComponent<InterfaceEntry>();
        if (entry == null)
            Debug.LogError("Could not add entry to interface. Entries must contain the InterfaceEntry component.");
        entry.SetEntry(name, description);

    }
   public void ClearEntries()
    {
        for (int i = 0; i < displayingIds.Count; i++)
        {
            Pool.FreeObject(displayingIds[i]);
        }
    }

}
