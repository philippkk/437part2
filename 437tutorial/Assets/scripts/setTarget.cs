using UnityEngine;

public class setTarget : MonoBehaviour
{
    public GameObject targetObject;
    
    public float maxRaycastDistance = 100f;
    
    public LayerMask groundLayer;


    void FixedUpdate()
    {
        ShootRaycast();
    }
    
    void ShootRaycast()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            if (hit.collider.CompareTag("ground"))
            {
                targetObject.transform.position = hit.point;
                
                Debug.DrawLine(transform.position, hit.point, Color.green);
            }
        }
    }
}
