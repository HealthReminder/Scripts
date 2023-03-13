using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Render the camera into a texture and then into a material
/// </summary>
public class BackgroundRenderer : MonoBehaviour
{
    public Camera BackgroundCamera;     //Camera from which the render texture will be generated
    public Material BackgroundMaterial; //Material that will be rendering the camera

    void Update()
    {
        // Get the render texture that the background camera is rendering to
        RenderTexture renderTexture = BackgroundCamera.targetTexture;
        // Check if the render texture exists
        if (renderTexture != null)
        {
            // Update the material's main texture with the render texture from the background camera
            BackgroundMaterial.mainTexture = renderTexture;
        }
    }
}
