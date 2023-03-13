using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spawns at random positions in a circle within a given radius using the Pooling class
/// </summary>
public class CircleSpawner : MonoBehaviour
{
    // Radius of the circle within which the objects will be spawned
    public float CircleRadius = 5;
    // Size of the object pool that will be used to instantiate the objects
    public int PoolSize = 25;
    // Total duration of the object shower
    public float ShowerDuration;
    // Animation curve that defines the rate at which objects will be spawned during the shower
    public AnimationCurve ShowerCurve;
    // Prefab that will be instantiated to create the objects
    public Transform Prefab;
    // The layer that the object pool will be assigned to
    public int PoolLayer;
    // Object pool that will manage the instantiated objects
    private Pooling<Transform> _pool;


    private void Awake()
    {
        // Create an object pool with the given prefab, pool size, transform and pool layer
        _pool = new Pooling<Transform>(Prefab, PoolSize, transform, PoolLayer);
    }

    // Instantiate the objects according to the shower settings
    [ContextMenu("Do Shower")]
    public void InstantiateTimed()
    {
        // Start a new coroutine to spawn the objects
        StartCoroutine(SpawnRoutine());
    }

    // Current time and progress in the object shower
    // For debug reasons (can be moved into the routine)
    float time = 0.0f;
    float progress = 0.0f;

    // Coroutine that spawns the objects according to the shower settings
    IEnumerator SpawnRoutine()
    {
        // Reset the time and progress variables
        time = 0.0f;
        progress = 0.0f;

        // Variables used to control the timing of the object spawns
        float waitTime;
        while (progress < 1)
        {
            // Spawn an object at a random position within the circle radius
            InstantiateRandom();

            // Update the progress based on the current time and the shower duration
            progress = Mathf.InverseLerp(0, ShowerDuration, time);

            // Get the wait time for the next object spawn based on the current progress and the shower curve
            waitTime = Mathf.Abs(ShowerCurve.Evaluate(progress));

            time += waitTime;

            yield return new WaitForSeconds(waitTime);
        }
        yield break;
    }

    // Instantiate an object in a random position in the circle
    public void InstantiateRandom()
    {
        // Get a random angle within the circle and calculate the spawn position based on that angle and the circle radius
        float angleInCircle = Random.Range(0f, 360f);
        Vector3 spawnPosition = transform.position + (Quaternion.Euler(0f, angleInCircle, 0f) * Vector3.forward * CircleRadius);

        // Get an object from the object pool and set its position and rotation
        _pool.GetFromPool(spawnPosition, Quaternion.identity);
    }

    // Draw a sphere with the circle radius in the editor preview, when selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, CircleRadius);
    }
}