using UnityEngine;

public class bodyController : MonoBehaviour
{
    [SerializeField] private Transform headTransform;    
    [SerializeField] private Transform[] legRoots;       
    [SerializeField] private Transform[] legEnds;       
    [SerializeField] private float heightOffset = 0.5f;  
    [SerializeField] private float rotationSpeed = 8f;  
    [SerializeField] private LayerMask groundMask;       
    
    private Vector3 initialHeadOffset;                   
    private Vector3 surfaceNormal = Vector3.up;          

    void Start()
    {
            initialHeadOffset = headTransform.position - transform.position;
    }

    void Update()
    {
        AdjustBodyHeight();
        AdjustBodyRotation();
    }
    
    private void AdjustBodyHeight()
    {
        if (legEnds == null || legEnds.Length == 0)
            return;
            
        float averageHeight = 0f;
        int validLegCount = 0;
        
        surfaceNormal = Vector3.up; 
        
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
        
        if (validLegCount >= 3)
        {
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

            Vector3 newPosition = transform.position;
            newPosition.y = averageHeight + heightOffset;
            
            Vector3 oldPosition = transform.position;
            
            transform.position = newPosition;
            
            float bodyYDelta = transform.position.y - oldPosition.y;
            Vector3 headPos = headTransform.position;
            headPos.y += bodyYDelta; 
            headTransform.position = headPos;
        }
    }
    
    private void AdjustBodyRotation()
    {
        if (surfaceNormal == Vector3.zero)
            return;
            
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
