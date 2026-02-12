using UnityEngine;

[CreateAssetMenu(fileName = "Q1VertexData", menuName = "Symmetric/Q1 Vertex Data")]
public class Q1VertexData : ScriptableObject
{
    [SerializeField] public int quadrantVertexCount = 5;

    // Fixed 6 vertices (can be expand as needed)
    [SerializeField] public Vector2 v0 = new Vector2(-1.5f, 0f);
    [SerializeField] public Vector2 v1 = new Vector2(-1.5f, 2f);
    [SerializeField] public Vector2 v2 = new Vector2(-3f, 3f);
    [SerializeField] public Vector2 v3 = new Vector2(-4f, 4f);
    [SerializeField] public Vector2 v4 = new Vector2(0f, 4.5f);
    [SerializeField] public Vector2 v5 = new Vector2(0f, 3.5f);  // Extra
    [SerializeField] public Vector2 v6;
    [SerializeField] public Vector2 v7;
    [SerializeField] public Vector2 v8;
    [SerializeField] public Vector2 v9;
    [SerializeField] public Vector2 v10;
    [SerializeField] public Vector2 v11;
    [SerializeField] public Vector2 v12;
    [SerializeField] public Vector2 v13;
    [SerializeField] public Vector2 v14;
    [SerializeField] public Vector2 v15;
    [SerializeField] public Vector2 v16;
    [SerializeField] public Vector2 v17;
    [SerializeField] public Vector2 v18;
    [SerializeField] public Vector2 v19;

    public Vector2[] GetVertices()
    {
        Vector2[] verts = new Vector2[quadrantVertexCount];
        Vector2[] all = { v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19 };
        for(int i = 0; i < quadrantVertexCount && i < all.Length; i++)
            verts[i] = all[i];
        return verts;
    }

    public void SetVertex(int i, Vector2 pos)
    {
        switch(i)
        {
            case 0:
                v0 = pos;
                break;
            case 1:
                v1 = pos;
                break;
            case 2:
                v2 = pos;
                break;
            case 3:
                v3 = pos;
                break;
            case 4:
                v4 = pos;
                break;
            case 5:
                v5 = pos;
                break;
            case 6:
                v6 = pos;
                break;
            case 7:
                v7 = pos;
                break;
            case 8:
                v8 = pos;
                break;
            case 9:
                v9 = pos;
                break;
            case 10:
                v10 = pos;
                break;
            case 11:
                v11 = pos;
                break;
            case 12:
                v12 = pos;
                break;
            case 13:
                v13 = pos;
                break;
            case 14:
                v14 = pos;
                break;
            case 15:
                v15 = pos;
                break;
            case 16:
                v16 = pos;
                break;
            case 17:
                v17 = pos;
                break;
            case 18:
                v8 = pos;
                break;
            case 19:
                v19 = pos;
                break;
        }
    }
}
