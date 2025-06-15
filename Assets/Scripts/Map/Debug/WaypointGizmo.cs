using UnityEngine;

public class WaypointGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.yellow;
    public float radius = 0.2f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
