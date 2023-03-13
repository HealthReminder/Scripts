using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Copy the rotation of another transform
/// </summary>
public class FollowRotation : MonoBehaviour
{
    public Transform TargetTransform;
    private void Update()
    {
        transform.rotation = TargetTransform.rotation;
    }
}
