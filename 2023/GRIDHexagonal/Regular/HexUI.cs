using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class HexUI : MonoBehaviour
{
    [Tooltip("The object representing the hexagonal cell in world space")]
    public GameObject hexPrefab;
    [Tooltip("Dictionary relating coordinates to the gameobject instantiated in world space")]
    public Dictionary<Vector2, HexUICell> coordToHex;
    //public AnimationCurve verticalMovementOffset;

    /// <summary>
    /// Spawns the objects that make up the hex map
    /// </summary>
    public void Instantiate(Vector2[] allCoords)
    {
        coordToHex = new Dictionary<Vector2, HexUICell>();
        for (int i = 0; i < allCoords.Length; i++)
        {
            Vector2 coord = allCoords[i];
            GameObject newObj = Instantiate(hexPrefab, new Vector3(coord.y * 1.0f, 0, coord.x * 1.00f), Quaternion.identity, transform);
            if (coord.y % 2 == 0)
                newObj.transform.position += new Vector3(0, 0, 0.5f);
            newObj.name = coord.ToString();
            newObj.transform.parent = transform;
            HexUICell hex = newObj.GetComponent<HexUICell>();
            hex.Coordinates = coord;
            coordToHex.Add(coord, hex);
        }
    }
    /// <summary>
    /// Trigger the visibility of hexagonal cells
    /// </summary>
    /// <param name="allCoords">A list of coords to make invisible</param>
    public void TriggerVisibility(List<Vector2> allCoords, bool isVisible)
    {
        for (int i = 0; i < allCoords.Count; i++)
        {
            coordToHex[allCoords[i]].IsVisible(isVisible);
        }
    }
  
    /// <summary>
    /// Change the colour of the hex objects
    /// </summary>
    /// <param name="hexes">An array of game objects from which a renderer will be accessed</param>
    /// <param name="newColor">The new color applied to all game objects</param>
    public void ChangeColour(Vector2[] hexes, Color newColor)
    {
        for (int i = 0; i < hexes.Length; i++)
            coordToHex[hexes[i]].ChangeColour(newColor);
    }
    /// <summary>
    /// DEBUG function. Show directions of a vector field.
    /// </summary>
    /// <param name="coordToDirections"></param>
    /// <param name="color"></param>
    public void ShowDirections(Dictionary<Vector2, Vector2> coordToDirections, Color color)
    {
        Vector2[] coords = coordToDirections.Keys.ToArray();
        for (int i = 0; i < coords.Length; i++)
        {
            if (coordToHex.ContainsKey(coords[i]))
            {
                Vector3 objPos = coordToHex[coords[i]].transform.position + new Vector3(0, 2, 0);
                Vector3 dir = new Vector3(coordToDirections[coords[i]].x, 0, coordToDirections[coords[i]].y);
                Debug.DrawRay(objPos, dir, color, 4.0f);
            }

        }
    }




}
