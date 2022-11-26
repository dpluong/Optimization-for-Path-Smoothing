using UnityEngine;
using UnityEditor;

public class Node : MonoBehaviour
{

    public float radius;
    
    private void OnDrawGizmos() 
    {
        Handles.color = Color.blue;
        Handles.DrawWireDisc(this.transform.position, new Vector3(0f, 1f, 0f), radius);
        Gizmos.DrawSphere(this.transform.position, 3);
    }

    public void SetNodeRadius(float r)
    {
        radius = r;
    }
}