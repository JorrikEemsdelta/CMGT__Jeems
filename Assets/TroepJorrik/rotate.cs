using UnityEngine;

public class rotate : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Rotation speed in degrees per second around the Z axis. Positive = clockwise in Unity's coordinate system.")]
    private float rotationSpeed = 15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate by rotationSpeed degrees per second around Z
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

    }
}
