using UnityEngine;

public class LightNew : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        vertices = new[]
        {
        new Vector3(0, 0,0),
        new Vector3(0, 10, 0),
        new Vector3(10,0, 0),
    };
        mesh.vertices = vertices;
        triangles = new[] {0,1,2};
        mesh.triangles = triangles;
    }
}
