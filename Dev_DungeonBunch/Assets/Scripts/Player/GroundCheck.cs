using Unity.VisualScripting;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Spherecast")]
    [SerializeField] float radius = 1f;
    [SerializeField] float maxDistance = 1f;
    [SerializeField] Transform origin;
    [SerializeField] LayerMask layerMask;
    
    private Ray DownRay => new(origin.position, Vector3.down);
    
    public bool isGrounded()
    {
        return Physics.SphereCast(DownRay, radius, maxDistance, layerMask);
    }

    public bool isGrounded(out RaycastHit hit)
    {
        return Physics.SphereCast(DownRay, radius, out hit, maxDistance, layerMask);
    }

    private void LateUpdate() {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gameObject.transform.position, Vector3.one);
    }
}
