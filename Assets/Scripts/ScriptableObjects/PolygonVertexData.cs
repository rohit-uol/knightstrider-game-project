using UnityEngine;

[CreateAssetMenu(fileName = "PolygonVertexData", menuName = "Symmetric/Polygon Vertex Data")]
public class PolygonVertexData : ScriptableObject
{
    [SerializeField] public Vector3[] worldVertices;  
    [SerializeField] public int q1VertexCount;  
}
