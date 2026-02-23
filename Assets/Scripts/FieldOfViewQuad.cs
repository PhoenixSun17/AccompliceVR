using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewQuad : MonoBehaviour
{
    public float fovHorizontal = 90.0f;
    public float fovVertical = 60.0f;
    public float distance = 5.0f;
    public Texture texture;

    private void Start()
    {
        // Calculate the size of the quad based on the field of view and distance
        float quadWidth = 2 * distance * Mathf.Tan(Mathf.Deg2Rad * fovHorizontal / 2.0f);
        float quadHeight = 2 * distance * Mathf.Tan(Mathf.Deg2Rad * fovVertical / 2.0f);

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.name = "FieldOfViewQuad";

        // Calculate the vertices for the quad
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-quadWidth / 2, -quadHeight / 2, distance);
        vertices[1] = new Vector3(quadWidth / 2, -quadHeight / 2, distance);
        vertices[2] = new Vector3(-quadWidth / 2, quadHeight / 2, distance);
        vertices[3] = new Vector3(quadWidth / 2, quadHeight / 2, distance);

        // Calculate the uv coordinates for the quad
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0.0f, 1.0f);
        uv[1] = new Vector2(1.0f, 1.0f);
        uv[2] = new Vector2(0.0f, 0.0f);
        uv[3] = new Vector2(1.0f, 0.0f);

        // Create the triangles for the quad
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        // Assign the mesh data to the mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        // Apply the mesh to the MeshFilter component
        GetComponent<MeshFilter>().mesh = mesh;

        // Create a new material with the custom shader
        Material material = new Material(Shader.Find("Unlit/Texture"));

        // Set the texture on the material
        material.SetTexture("_MainTex", texture);

        // Apply the material to the MeshRenderer component
        GetComponent<MeshRenderer>().material = material;
    }
}
