using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexagonLogic
{
    //This class can create an hexagonal grid - The grid is a dictionary of coordinates and its neighbors
    public static Dictionary<Vector2, Vector2[]> GenerateHexagonalGrid(int sizeX, int sizeY)
    {
        Dictionary<Vector2, Vector2[]> coordinateToNeighbours = new Dictionary<Vector2, Vector2[]>();
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
                coordinateToNeighbours.Add(new Vector2(x, y), GetNeighbours(new Vector2(x, y), sizeX, sizeY));
        return coordinateToNeighbours;
    }
    private static Vector2[] GetNeighbours(Vector2 coords, int sizeX, int sizeY)
    {
        //Returns the neighbors of a coordinate in a grid
        Vector2[] neighbours;
        if (coords.y % 2 != 0)
        {
            neighbours = new Vector2[]
            {
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(-1,1),
            new Vector2(-1,0),
            new Vector2(-1,-1),
            new Vector2(0,-1)
            };
        }
        else
        {
            neighbours = new Vector2[]
            {
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1),
            new Vector2(-1,0),
            new Vector2(0,-1),
            new Vector2(1,-1)
            };
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2 currentNeighbour = coords + neighbours[i];
            if (currentNeighbour.x < 0 || currentNeighbour.x >= sizeX || currentNeighbour.y < 0 || currentNeighbour.y >= sizeY)

                neighbours[i] = new Vector2(-1, -1);
            else
                neighbours[i] = currentNeighbour;
        }

        return neighbours;
    }
    #region Debug Functions
    private static void DebugNeighbours(Vector2 coord, int sizeX, int sizeY)
    {
        string t = "";
        Vector2[] n = GetNeighbours(coord, sizeX, sizeY);
        for (int i = 0; i < n.Length; i++)
            t += n[i] + "\n";
        Debug.Log(t);
    }
    private static void DebugGrid(Vector2[][][] grid)
    {
        //The debugging of this grid is inverted to match what it would look like in the application
        string t = "";
        for (int x = grid.Length - 1; x >= 0; x--)
        {
            for (int y = 0; y < grid[0].Length; y++)
            {
                for (int z = 0; z < 6; z++)
                {
                    t += grid[x][y][z] + " ";
                }
            }
            t += "\n";
        }
        Debug.Log(t);

        #endregion
    }
}


