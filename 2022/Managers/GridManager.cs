using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GridManager manages and stores data of
/// Hexagonal grids in form of a dictionary
/// The dictionary contains all coordinates needed to
/// Run a game with hexagonal coordinates
/// </summary>
public class GridManager
{
    [Tooltip("Hex coordinates and its neighbors")] 
    public Dictionary<Vector2, Vector2[]> coordinateToNeighbors; ///This dictionary relates map coordinates to its neighbors
    public GridManager(Vector2 size)
    {
        //Upon initialization the manager will generate a new hexagonal grid of given size and store it
        coordinateToNeighbors = new Dictionary<Vector2, Vector2[]>();
        coordinateToNeighbors = HexagonLogic.GenerateHexagonalGrid((int)size.x,(int)size.y); ///Generate logic for a hex map

        foreach (Vector2 v in coordinateToNeighbors[new Vector2(0, 0)])
            Debug.Log($"(0,0) : {v}");
        foreach (Vector2 v in coordinateToNeighbors[new Vector2(4, 4)])
            Debug.Log($"(4,4) : {v}");



        Debug.Log($"Grid created. Coordinate count {coordinateToNeighbors.Keys.Count}");
    }

}
