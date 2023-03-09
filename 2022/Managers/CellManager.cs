using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Responsible for managing all cells. Cells are visual objects
/// In the case of this project cells are the hex objects
/// The ones the player can click on*
/// </summary>
[System.Serializable]public class CellManager : MonoBehaviour
{
    public GameObject hexPrefab;
    public Dictionary<Vector2, GameObject> coordToObject;
    public AnimationCurve VerticalMovementWobble;
    public void Initiate(Vector2[] gridCoords, Dictionary<Vector2, float> heightMap)
    {
        coordToObject = new Dictionary<Vector2, GameObject>();
        SpawnMap(gridCoords);
        ChangeCellHeight(heightMap);
    }
    /// <summary>
    /// Spawns the objects that compose the hex map
    /// </summary>
    private void SpawnMap(Vector2[] allCoords)
    {
        for (int i = 0; i < allCoords.Length; i++)
        {
            Vector2 coord = allCoords[i];
            GameObject newObj = Instantiate(hexPrefab, new Vector3(coord.y,0, coord.x), Quaternion.identity, transform);
            if (coord.y % 2 == 0)
                newObj.transform.position += new Vector3(0, 0, 0.5f);
            newObj.name = coord.ToString();
            coordToObject.Add(coord, newObj);
        }
    }
    public void HideOutsideRadius(int radius, Vector2 midPoint)
    {
        if (coordToObject == null)
            Debug.LogError("Cannot hide null objects");
        Vector2[] coords = coordToObject.Keys.ToArray();
        for (int i = 0; i < coords.Length; i++)
        {
            if(Vector2.Distance(coords[i], midPoint) > radius)
                coordToObject[coords[i]].SetActive(false);
        }
    }
    /// <summary>
    /// Change the height of the cells on the map
    /// Has the option of moving them instantly or gradually
    /// </summary>
    /// <param name="heightMap">Coordinates to height; Heightmap</param>
    /// <param name="speed">From 0-1, how quickly the cells move to their new positions</param>
    public void ChangeCellHeight(Dictionary<Vector2, float> heightMap, float speed = 1)
    {
        //speed = 1;
        Vector2[] coords = heightMap.Keys.ToArray();
        for (int i = 0; i < coords.Length; i++)
        {
            Vector2 currentCoord = coords[i];
            if (coordToObject.ContainsKey(currentCoord))
            {
                GameObject obj = coordToObject[currentCoord];
                Vector3 currentPos = obj.transform.position;
                Vector3 newPos = new Vector3(currentPos.x, heightMap[currentCoord], currentPos.z);
                StartCoroutine(CellMovementRoutine(obj.transform, newPos, speed));
                ///obj.transform.position = new Vector3(currentPos.x, heightMap[currentCoord], currentPos.z);
            }
                
        }
    }
    IEnumerator CellMovementRoutine(Transform cellTransform,Vector3 newPos, float speed)
    {
        float progress = 0;
        //This is a tricky one watch out
        Vector3 oldPos = cellTransform.position;
        while (progress <= 1)
        {
            cellTransform.position = Vector3.Lerp(oldPos, newPos, VerticalMovementWobble.Evaluate(progress));
            progress += 1 * speed;
            yield return null;
        }
        cellTransform.position = newPos;
        yield break;
    }
    public void ChangeCellSingleColor(Vector2[] cells, Color newColor)
    {
        GameObject currentObj;
        MaterialPropertyBlock propBlock;
        propBlock = new MaterialPropertyBlock();
        propBlock.SetColor("_Color", newColor);
        for (int i = 0; i < cells.Length; i++)
        {
            currentObj = coordToObject[cells[i]];
            currentObj.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(propBlock); //This is horrible I know
        }
    }
    public void ChangeCellColorHeatmap(Dictionary<Vector2, float> heatmap)
    {
        GameObject currentObj;
        MaterialPropertyBlock propBlock;
        propBlock = new MaterialPropertyBlock();
        Vector2[] cells = heatmap.Keys.ToArray();
        for (int i = 0; i < cells.Length; i++)
        {
            propBlock.SetColor("_Color", new Color(heatmap[cells[i]],0, 1-heatmap[cells[i]]));
            currentObj = coordToObject[cells[i]];
            currentObj.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(propBlock); 
        }
    }
}
