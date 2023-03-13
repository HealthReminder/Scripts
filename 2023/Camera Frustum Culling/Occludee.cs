using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Occludee class is necessary for occlusion culling to happen with procedural objects
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Occludee : MonoBehaviour
{
    /// <summary>
    /// Add itself to the Occluder to enable occlusion
    /// </summary>
    private void Start()
    {
        FindObjectOfType<FrustumCulling>().AddOccludee(GetComponent<Renderer>());
    }
}
