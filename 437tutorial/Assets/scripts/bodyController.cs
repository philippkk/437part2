using UnityEngine;

public class bodyController : MonoBehaviour
{
    [SerializeField] private Transform headTransform;    // Reference to the spider's head
    [SerializeField] private Transform[] legRoots;       // References to the start points of each leg
    [SerializeField] private Transform[] legEnds;        // References to the end points of each leg
    [SerializeField] private float heightOffset = 0.5f;  // Additional height offset from average leg end positions
    [SerializeField] private float rotationSpeed = 8f;   // Speed at which the body rotates to align with the surface
    [SerializeField] private LayerMask groundMask;       // Layer mask for ground detection
    
    private Vector3 initialHeadOffset;                   // Initial offset from body to head
    private Vector3 surfaceNormal = Vector3.up;          // Normal vector of the surface below the spider

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Store the initial offset between the body and head
        if (headTransform != null)
        {
            initialHeadOffset = headTransform.position - transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // DetectSurfaceOrientation();
        AdjustBodyHeight();
        AdjustBodyRotation();
    }
    
    // Calculate and set the body height based on average leg end positions
    private void AdjustBodyHeight()
    {
        if (legEnds == null || legEnds.Length == 0)
            return;
            
        // Calculate average height of leg ends
        float averageHeight = 0f;
        int validLegCount = 0;
        
        // Calculate surface normal based on leg positions
        surfaceNormal = Vector3.up; // Default to up
        
        // Get all leg end positions
        Vector3[] positions = new Vector3[legEnds.Length];
        for (int i = 0; i < legEnds.Length; i++)
        {
            if (legEnds[i] != null)
            {
                positions[i] = legEnds[i].position;
                averageHeight += positions[i].y;
                validLegCount++;
            }
        }
        
        // Calculate normal of the surface if we have at least 3 points
        if (validLegCount >= 3)
        {
            // Get three non-collinear points to calculate normal
            Vector3 v1 = positions[1] - positions[0];
            Vector3 v2 = positions[2] - positions[0];
            surfaceNormal = Vector3.Cross(v1, v2).normalized;
            
            // Ensure normal points up
            if (surfaceNormal.y < 0)
                surfaceNormal = -surfaceNormal;
        }

        if (validLegCount > 0)
        {
            averageHeight /= validLegCount;

            // Set body position with the calculated height plus offset
            Vector3 newPosition = transform.position;
            newPosition.y = averageHeight + heightOffset;
            
            // Store old body position for head adjustment calculation
            Vector3 oldPosition = transform.position;
            
            // Apply new body position
            transform.position = newPosition;
            
            // Calculate how much the body moved vertically in world space
            float bodyYDelta = transform.position.y - oldPosition.y;
            
            // Update the head position to maintain its relative position
            Vector3 headPos = headTransform.position;
            headPos.y += bodyYDelta; // Add the same Y delta that the body moved
            headTransform.position = headPos;
        }
    }
    
    // Adjust body rotation to align with surface
    private void AdjustBodyRotation()
    {
        if (surfaceNormal == Vector3.zero)
            return;
            
        // Calculate target rotation to align with surface normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        
        // Smoothly rotate towards target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
