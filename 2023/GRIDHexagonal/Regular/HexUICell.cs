using UnityEngine;
public class HexUICell : MonoBehaviour
{
    [Tooltip("Hexagonal coordinate for location in hexagonal grid")]
    public Vector2 Coordinates;
    [Tooltip("Renderers for colour changing")]
    public MeshRenderer[] Renderers;
    /// <summary>
    /// Change the colour of this specific hexagonal cell
    /// </summary>
    /// <param name="newColor">The new colour applied to the cell</param>
    public void ChangeColour(Color newColor)
    {
        MaterialPropertyBlock _propBlock;
        //Using property blocks
        _propBlock = new MaterialPropertyBlock();
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_Color", newColor);
            //Randomize colours to see if the property block shader is working as intended
            //_propBlock.SetColor("_Color",new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1));
            Renderers[i].SetPropertyBlock(_propBlock);
        }
        //Without property blocks
        //for (int i = 0; i < Renderers.Length; i++)
        //    Renderers[i].material.SetColor("_Color", newColor);
    }
    /// <summary>
    /// Trigger the visibility of the hexagonal cell
    /// </summary>
    /// <param name="isVisible">The new state of the cell's visibility</param>
    public void IsVisible(bool isVisible)
    {
        if (isVisible)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
    private Mesh _proceduralMesh;
   
}
