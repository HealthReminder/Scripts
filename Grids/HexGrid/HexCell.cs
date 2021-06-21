using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HexCell : MonoBehaviour
{
    [HideInInspector] public Vector3 coord_raw;
    public Vector3 coord_directional;
    public Text text_coordinates;
    public HexMesh mesh;
    public void DebugCoordinates()
    {
        text_coordinates.gameObject.SetActive(true);
        text_coordinates.text = coord_directional.x.ToString() + "\n" + coord_directional.y.ToString() + "\n" + coord_directional.z.ToString();

    }
}
