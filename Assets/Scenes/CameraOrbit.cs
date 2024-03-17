using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target; // The target object to look at (center of rotation)
    public float orbitSpeed = 10.0f; // Speed of rotation around the target

    private Vector3 targetOffset;

    void Start()
    {
        if (target != null)
        {
            // Calculate the initial offset from target, but keep the camera's original height
            targetOffset = new Vector3(0, transform.position.y, 0) - target.position;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Rotate around the target at orbitSpeed, but keep the original elevation
            transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
            
            
            transform.LookAt(target);

            
        }
    }
}
