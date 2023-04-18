using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Collision event invokes functions that have a float for magnitude
/// And a vector3 for position as a parameter
/// The magnitude will determine the audio volume and the position will
/// Determine the position of the audio source
/// </summary>

[System.Serializable] public class CollisionEvent : UnityEvent<(float,Vector3)>
{
    
}
/// <summary>
/// Invokes a Collision Event on collision
/// </summary>
public class OnCollisionEvent : MonoBehaviour
{
    [SerializeField] private float _minMagnitude;
    public CollisionEvent CollisionEvents;
    private void OnCollisionEnter(Collision collision)
    {
        float mag = collision.relativeVelocity.magnitude;
        if (mag >= _minMagnitude)
            CollisionEvents.Invoke((mag, collision.contacts[0].point));
    }
}