using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles frustum culling and occlusion culling
/// Requires occludee to have an Occludee script
/// Occludees add themselves to this object when enabled
/// </summary>
public class FrustumCulling : MonoBehaviour
{
    public Camera Camera; /// Frustum planes will be calculated from this camera
    private List<Renderer> _renderers; /// Occludees 

    private void Awake()
    {
        _renderers = new List<Renderer>();
    }

    private void Update()
    {
        //Frustum Culling
        //Check if object is within camera frustum 
        for (int i = 0; i < _renderers.Count; i++)
        {
            Renderer rend = _renderers[i];
            if (rend != null)
            {
                if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera), rend.bounds))
                {
                    rend.enabled = true; // Object is visible, so enable rendering

                }
                else
                    rend.enabled = false; // Object is not visible, so disable rendering

            }
        }

    }
    /// <summary>
    /// Add a new occludee to the list
    /// </summary>
    /// <param name="renderer">Renderer instance</param>
    public void AddOccludee(Renderer renderer)
    {
        _renderers.Add(renderer);
    }
}
