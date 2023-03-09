using UnityEngine;
using System.Collections.Generic;

public static class HexagonalCoordinates
{
    // This class can create an hexagonal grid - The grid is a dictionary of coordinates and its neighbors

    /// <summary>
    /// Generates a hexagonal grid of the given size and returns a dictionary of coordinates and their neighbors.
    /// </summary>
    /// <param name="sizeX">The width of the grid.</param>
    /// <param name="sizeY">The length of the grid.</param>
    /// <param name="hexagonSizeX">The width of the hexagon.</param>
    /// <param name="hexagonSizeY">The length of the hexagon.</param>
    /// <returns>A dictionary of coordinates and their neighbors.</returns>
    public static Dictionary<Vector2, Vector2[]> GenerateHexagonalCoordinates(int sizeX, int sizeY)
    {
        Dictionary<Vector2, Vector2[]> coordinateToNeighbours = new Dictionary<Vector2, Vector2[]>();
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
                coordinateToNeighbours.Add(new Vector2(x, y), GetNeighbours(new Vector2(x, y), sizeX, sizeY));
        return coordinateToNeighbours;
    }

    /// <summary>
    /// Returns the neighbors of a coordinate in a grid.
    /// </summary>
    /// <param name="coords">The coordinate to get neighbors for.</param>
    /// <param name="gridSizeX">The width of the grid.</param>
    /// <param name="gridSizeY">The length of the grid.</param>
    /// <returns>An array of the coordinate's neighbors.</returns>
    private static Vector2[] GetNeighbours(Vector2 coords, int gridSizeX, int gridSizeY)
    {
        Vector2[] neighbours;
        // If the y-coordinate is odd, use these neighbors
        if (coords.y % 2 != 0)
        {
            neighbours = new Vector2[] {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 1),
                new Vector2(-1, 0),
                new Vector2(-1, -1),
                new Vector2(0, -1)
            };
        }
        // If the y-coordinate is even, use these neighbors
        else
        {
            neighbours = new Vector2[] {
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(-1, 0),
                new Vector2(0, -1),
                new Vector2(1, -1)
            };
        }
        // Check if each neighbor is within the grid bounds and replace invalid ones with (-1, -1)
        for (int i = 0; i < 6; i++)
        {
            Vector2 currentNeighbour = coords + neighbours[i];
            if (currentNeighbour.x < 0 || currentNeighbour.x >= gridSizeX || currentNeighbour.y < 0 || currentNeighbour.y >= gridSizeY)
                neighbours[i] = new Vector2(-1, -1);
            else
                neighbours[i] = currentNeighbour;
        }

        return neighbours;
    }

    #region Debug
    /// <summary>
    /// Log the neighbors of a coordinate in the hexagonal grid.
    /// </summary>
    /// <param name="coord">The coordinate to debug.</param>
    /// <param name="sizeX">The horizontal size of the hexagonal grid.</param>
    /// <param name="sizeY">The vertical size of the hexagonal grid.</param>
    public static void DebugNeighbours(Vector2 coord, int sizeX, int sizeY)
    {
        string t = "";
        Vector2[] n = GetNeighbours(coord, sizeX, sizeY);
        for (int i = 0; i < n.Length; i++)
        {
            t += n[i] + "\n";
        }
        Debug.Log(t);
    }
    #endregion
}