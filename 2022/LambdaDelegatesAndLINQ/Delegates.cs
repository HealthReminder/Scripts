using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delegates : MonoBehaviour
{
    float elapsed_time;
    public int[,,,] four_dimensional_array;


    private void Start()
    {
        elapsed_time = 0;
        four_dimensional_array = new int[1000, 10, 10, 10];
        LoopThroughArray((t, x, y, z) => four_dimensional_array[t, x, y, z] = 1);
        LoopThroughArray( (t,x,y,z) => Debug.Log(four_dimensional_array[t, x, y, z]));
        Debug.Log(elapsed_time / 1000.0f);
    }

    public void LoopThroughArray(Action<int,int,int,int> do_something)
    {
        //Last dimension could be considered time, and the main iterator
        for (int t = 0; t < four_dimensional_array.GetLength(0); t++)
            for (int x = 0; x < four_dimensional_array.GetLength(1); x++)
                for (int y = 0; y < four_dimensional_array.GetLength(2); y++)
                    for (int z = 0; z < four_dimensional_array.GetLength(3); z++)
                    {
                        do_something(t, x, y, z);
                        elapsed_time += Time.deltaTime;
                    }




    }
}
