using UnityEngine;

public class setTarget : MonoBehaviour
{
    // The target object to move
    public GameObject targetObject;
    
    // Maximum distance for the raycast
    public float maxRaycastDistance = 100f;
    
    // Layer mask for ground objects (optional)
    public LayerMask groundLayer;


    // Update is called once per frame
    void FixedUpdate()
    {
        // Perform raycast every frame
        ShootRaycast();
    }
    
    // Method to shoot a raycast and move target to hit point
    void ShootRaycast()
    {
        // Check if we have a target object
        if (targetObject == null)
            return;
            
        // Create a ray from the transform's position pointing downward
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        
        // Debug ray to visualize in Scene view
        Debug.DrawRay(transform.position, Vector3.down * maxRaycastDistance, Color.red);
        
        // Perform the raycast
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            // Check if the hit object has the tag "ground"
            if (hit.collider.CompareTag("ground"))
            {
                // Move the target object to the hit point
                targetObject.transform.position = hit.point;
                
                // Optional: Debug message
                Debug.DrawLine(transform.position, hit.point, Color.green);
                
                // You can uncomment this if you want to see the hit points in the console
                // Debug.Log("Hit ground at: " + hit.point);
            }
        }
    }
}
