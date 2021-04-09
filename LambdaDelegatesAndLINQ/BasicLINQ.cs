using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicLINQ : MonoBehaviour
{
    List<GameObject> instantiated_objects;
    public GameObject closest_object;
    private void Start()
    {
        instantiated_objects = new List<GameObject>();
        //Instantiate some objects randomly around this object as children
        for (int i = 0; i < 15; i++)
        {
            instantiated_objects.Add(Instantiate(new GameObject(), new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)), Quaternion.identity, transform));
        }

        //Calls non LINQ Function
        //closest_object = GetClosestObject(instantiated_objects, transform.position);

        //Calls function using LINQ and lambda operators
        var linq_closest_object = instantiated_objects.OrderBy(o => Vector3.Distance(transform.position, o.transform.position)).FirstOrDefault();
        closest_object = linq_closest_object;
        //Lists are iteratable and you can use lambda operators
        //To assign functions to the orderBy action

    }

    /// NON-LINQ FUNCTION EXAMPLE
    /*
    public GameObject GetClosestObject (List<GameObject> all_objects, Transform relation_to)
    {
        GameObject closest_object = null;

        float smallest_distance = 999999f;
        foreach (GameObject o in all_objects)
        {
            var dist = Vector3.Distance(relation_to.position, o.transform.position);
            if(dist < smallest_distance)
            {
                smallest_distance = dist;
                closest_object = o;
            }
        }
        return closest_object;
    }*/

}
