using UnityEditor;
using UnityEngine;

public class CreateTileInEditMode : MonoBehaviour
{
    public float horizScaleFactor = 0.05f;
    public Mesh baseMesh;
    public Mesh edgeMeshX;
    public Mesh edgeMeshZ;
    public MeshFilter meshFilter;



    [ContextMenu("Grab Mesh From Filter")]
    void GrabMeshFromFilter ()
    {
        meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        AssetDatabase.CreateAsset(mesh, "Assets/GrabbedMesh.asset");
    }

    [ContextMenu("Output All Vertices")]
    void OutputVertices ()
    {
        Vector3[] baseMeshVertices = baseMesh.vertices;
        //Vector2[] baseMeshUvs = baseMesh.uv;
        //Vector2[] baseMeshUv2s = baseMesh.uv2;
        int[] baseMeshTriangles = baseMesh.triangles;


        for ( int i = 0; i < baseMeshVertices.Length; i++ )
        {
            //Debug.Log("[" + (i + 1) + "] " + baseMeshVertices[i].ToString() + ". UV: " + baseMeshUvs[i].ToString() + ". UV2: " + baseMeshUv2s[i].ToString());
            Debug.Log("[" + (i + 1) + "] " + baseMeshVertices[i].ToString());
        }

        int count = 0;
        while ( count < baseMeshTriangles.Length )
        {
            string thisTriangle = "(" + baseMeshTriangles[count] + ", " + baseMeshTriangles[count + 1] + ", " + baseMeshTriangles[count + 2] + ")";
            count += 3;
            Debug.Log("[" + Mathf.RoundToInt(count / 3f) + "] " + thisTriangle);
        }
    }

    [ContextMenu("Create Simple Mesh")]
    void CreateSimpleMesh ()
    {
        Vector3[] newMeshVertices = new Vector3[4];

        newMeshVertices[0] = Vector3.forward + Vector3.right;
        newMeshVertices[2] = Vector3.right;



        for ( int i = 0; i < newMeshVertices.Length; i++ )
        {
            newMeshVertices[i] = newMeshVertices[i] - 0.5f * Vector3.right - 0.5f * Vector3.forward;
        }

        for ( int i = 1; i < newMeshVertices.Length; i += 2 )
        {
            newMeshVertices[i] = newMeshVertices[i - 1] * 0.99f;
        }


        int[] newMeshTriangles = new int[] {
            0, 2, 3, 0, 3, 1
        };


        Mesh mesh = new Mesh();
        mesh.name = "Simple Mesh";

        mesh.vertices = newMeshVertices;
        mesh.triangles = newMeshTriangles;

        AssetDatabase.CreateAsset(mesh, "Assets/SimpleMesh.asset");

    }

    [ContextMenu("Create Ring Mesh")]
    void CreateRingMesh ()
    {
        Vector3[] horizMeshVertices = edgeMeshX.vertices;
        int[] horizMeshTriangles = edgeMeshX.triangles;

        Vector3[] newMeshVertices = new Vector3[8];

        newMeshVertices[0] = Vector3.zero;
        newMeshVertices[2] = Vector3.forward;
        newMeshVertices[4] = Vector3.forward + Vector3.right;
        newMeshVertices[6] = Vector3.right;

        for ( int i = 0; i < newMeshVertices.Length; i++ )
        {
            newMeshVertices[i] = newMeshVertices[i] - 0.5f * Vector3.right - 0.5f * Vector3.forward;
        }

        for ( int i = 1; i < 8; i += 2 )
        {
            newMeshVertices[i] = newMeshVertices[i - 1] * 0.99f;
        }


        horizMeshTriangles = new int[] {
            0, 2, 1, 3, 1, 2,
            2, 4, 3, 5, 3, 4,
            4, 6, 5, 7, 5, 6,
            6, 0, 7, 1, 7, 0
        };


        Mesh mesh = new Mesh();
        mesh.name = "Ring Mesh";

        mesh.vertices = newMeshVertices;
        mesh.triangles = horizMeshTriangles;

        AssetDatabase.CreateAsset(mesh, "Assets/RingMesh.asset");

    }

    [ContextMenu("Horizontal Scale Mesh")]
    void HorizontalScale ()
    {
        Vector3[] baseMeshVertices = baseMesh.vertices;
        Vector2[] baseMeshUvs = baseMesh.uv;
        Vector2[] baseMeshUv2s = baseMesh.uv2;
        int[] baseMeshTriangles = baseMesh.triangles;

        Vector3[] newMeshVertices = new Vector3[baseMeshVertices.Length];

        for ( int i = 0; i < baseMeshVertices.Length; i++ )
        {
            newMeshVertices[i] = new Vector3(baseMesh.vertices[i].x * horizScaleFactor, baseMesh.vertices[i].y, baseMesh.vertices[i].z);
        }



        Mesh mesh = new Mesh();
        mesh.name = "Scaled Mesh";

        mesh.vertices = newMeshVertices;
        mesh.uv = baseMeshUvs;
        mesh.uv2 = baseMeshUv2s;
        mesh.triangles = baseMeshTriangles;

        AssetDatabase.CreateAsset(mesh, "Assets/ScaledMesh.asset");

    }

    [ContextMenu("Rotate Mesh")]
    void RotateMesh ()
    {
        Vector3[] baseMeshVertices = baseMesh.vertices;
        Vector2[] baseMeshUvs = baseMesh.uv;
        Vector2[] baseMeshUv2s = baseMesh.uv2;
        int[] baseMeshTriangles = baseMesh.triangles;

        Vector3[] newMeshVertices = new Vector3[baseMeshVertices.Length];

        for ( int i = 0; i < baseMeshVertices.Length; i++ )
        {
            newMeshVertices[i] = new Vector3(baseMesh.vertices[i].z, baseMesh.vertices[i].y, baseMesh.vertices[i].x);
        }

        Mesh mesh = new Mesh();
        mesh.name = "Rotated Mesh";

        mesh.vertices = newMeshVertices;
        mesh.uv = baseMeshUvs;
        mesh.uv2 = baseMeshUv2s;
        mesh.triangles = baseMeshTriangles;

        AssetDatabase.CreateAsset(mesh, "Assets/RotatedMesh.asset");

    }




    //[ContextMenu("Make Tile Mesh")]
    //void MakeTileMesh ()
    //{



    //    Mesh mesh = new Mesh();
    //    mesh.name = "Tile Mesh";

    //    Vector3[] vertices = new Vector3[16];
    //    vertices[4] = new Vector3(0f, tileThickness / 6f, 1f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[5] = new Vector3(1f, tileThickness / 6f, 1f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[6] = new Vector3(1f, tileThickness / 6f, 0f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[7] = new Vector3(0f, tileThickness / 6f, 0f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[0] = vertices[4] + inwardBevel * Vector3.right - inwardBevel * Vector3.forward + tileThickness / 6f * Vector3.up;
    //    vertices[1] = vertices[5] - inwardBevel * Vector3.right - inwardBevel * Vector3.forward + tileThickness / 6f * Vector3.up;
    //    vertices[2] = vertices[6] - inwardBevel * Vector3.right + inwardBevel * Vector3.forward + tileThickness / 6f * Vector3.up;
    //    vertices[3] = vertices[7] + inwardBevel * Vector3.right + inwardBevel * Vector3.forward + tileThickness / 6f * Vector3.up;
    //    vertices[8] = new Vector3(0f, -tileThickness / 6f, 1f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[9] = new Vector3(1f, -tileThickness / 6f, 1f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[10] = new Vector3(1f, -tileThickness / 6f, 0f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[11] = new Vector3(0f, -tileThickness / 6f, 0f) - 0.5f * Vector3.right - 0.5f * Vector3.forward;
    //    vertices[12] = vertices[8] + inwardBevel * Vector3.right - inwardBevel * Vector3.forward - tileThickness / 6f * Vector3.up;
    //    vertices[13] = vertices[9] - inwardBevel * Vector3.right - inwardBevel * Vector3.forward - tileThickness / 6f * Vector3.up;
    //    vertices[14] = vertices[10] - inwardBevel * Vector3.right + inwardBevel * Vector3.forward - tileThickness / 6f * Vector3.up;
    //    vertices[15] = vertices[11] + inwardBevel * Vector3.right + inwardBevel * Vector3.forward - tileThickness / 6f * Vector3.up;

    //    mesh.vertices = vertices;


    //    //Vector2[] uvVertices = new Vector2[4];
    //    //for ( int i = 0; i < uvVertices.Length; i++ )
    //    //    uvVertices[i] = new Vector2((vertices[i].x + wedgeRadius / 2) / wedgeRadius, vertices[i].z / wedgeHeight);
    //    //mesh.uv = uvVertices;

    //    Vector2[] uvs = new Vector2[vertices.Length];

    //    for ( int i = 0; i < uvs.Length; i++ )
    //    {
    //        uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
    //    }
    //    mesh.uv = uvs;
    //    mesh.uv2 = uvs;


    //    mesh.triangles = new int[] {
    //        0, 1, 2, 0, 2, 3,
    //        0, 4, 5, 0, 5, 1,
    //        1, 5, 6, 1, 6, 2,
    //        2, 6, 7, 2, 7, 3,
    //        3, 7, 4, 3, 4, 0,
    //        5, 4, 8, 8, 9, 5,
    //        6, 5, 9, 9, 10, 6,
    //        7, 6, 10, 10, 11, 7,
    //        4, 7, 11, 11, 8, 4,
    //        12, 13, 9, 12, 9, 8,
    //        13, 14, 10, 13, 10, 9,
    //        14, 15, 11, 14, 11, 10,
    //        15, 12, 8, 15, 8, 11,
    //        15, 14, 13, 15, 13, 12
    //      //12, 13, 14, 12, 14, 15
    //    };

    //    AssetDatabase.CreateAsset(mesh, "Assets/TileMesh.asset");
    //}

    //[ContextMenu("Make Wedge Particle Mesh")]
    //void MakeWedgeParticleMesh ()
    //{
    //    wedgeHeight = Mathf.Sqrt(Mathf.Pow(wedgeRadius, 2) - Mathf.Pow(wedgeRadius / 2, 2));
    //    centerHeight = Mathf.Sqrt(Mathf.Pow(centerRadius, 2) - Mathf.Pow(centerRadius / 2, 2));
    //    Mesh mesh = new Mesh();
    //    mesh.name = "Wedge Particle Mesh";

    //    Vector3[] vertices = new Vector3[8];
    //    vertices[2] = new Vector3(-wedgeRadius / 2, 0, wedgeHeight);
    //    vertices[0] = vertices[2] * centerRadius / wedgeRadius;
    //    vertices[4] = new Vector3(wedgeRadius / 2, 0, wedgeHeight);
    //    vertices[6] = vertices[4] * centerRadius / wedgeRadius;
    //    for ( int i = 1; i < vertices.Length; i += 2 )
    //        vertices[i] = vertices[i - 1] - Vector3.down * 0.1f;
    //    mesh.vertices = vertices;

    //    Vector2[] uvVertices = new Vector2[8];
    //    for ( int i = 0; i < uvVertices.Length; i++ )
    //        uvVertices[i] = new Vector2((vertices[i].x + wedgeRadius / 2) / wedgeRadius, vertices[i].z / wedgeHeight);
    //    mesh.uv = uvVertices;

    //    mesh.triangles = new int[] {
    //        0, 1, 2,    2, 1, 3,
    //        2, 3, 4,    4, 3, 5,
    //        4, 5, 6,    6, 5, 7,
    //        6, 7, 0,    0, 7, 1 };

    //    AssetDatabase.CreateAsset(mesh, "Assets/WedgeParticleMesh.asset");
    //}

    //[ContextMenu("Make Center Mesh")]
    //void MakeCenterMesh ()
    //{
    //    Mesh mesh = new Mesh();
    //    mesh.name = "Center Mesh";

    //    GameObject vertexTracer = new GameObject("Vertex Tracer");
    //    vertexTracer.transform.position = Vector3.zero;
    //    GameObject thisVertex = new GameObject("Vertex");
    //    thisVertex.transform.SetParent(vertexTracer.transform, false);
    //    thisVertex.transform.position = centerRadius * Vector3.RotateTowards(Vector3.forward, Vector3.right, 30f * Mathf.Deg2Rad, 0f);


    //    Vector3[] vertices = new Vector3[6];
    //    for ( int i = 0; i < vertices.Length; i++ )
    //    {
    //        vertexTracer.transform.Rotate(Vector3.up, 60f);
    //        vertices[i] = thisVertex.transform.position;
    //    }

    //    mesh.vertices = vertices;

    //    Vector2[] uvVertices = new Vector2[6];
    //    for ( int i = 0; i < uvVertices.Length; i++ )
    //    {
    //        uvVertices[i] = new Vector2((vertices[i].x + centerRadius / 2) / centerRadius, (vertices[i].y + centerRadius / 2) / centerRadius);
    //        Debug.Log(uvVertices[i].ToString());
    //    }
    //    mesh.uv = uvVertices;

    //    mesh.triangles = new int[] {
    //        0, 4, 5,
    //        0, 1, 4,
    //        1, 3, 4,
    //        1, 2, 3
    //    };

    //    AssetDatabase.CreateAsset(mesh, "Assets/CenterMesh.asset");
    //    DestroyImmediate(thisVertex);
    //    DestroyImmediate(vertexTracer);
    //}

    //[ContextMenu("Make Center Particle Mesh")]
    //void MakeCenterParticleMesh ()
    //{
    //    Mesh mesh = new Mesh();
    //    mesh.name = "Center Particle Mesh";

    //    GameObject vertexTracer = new GameObject("Vertex Tracer");
    //    vertexTracer.transform.position = Vector3.zero;
    //    GameObject thisVertex = new GameObject("Vertex");
    //    thisVertex.transform.SetParent(vertexTracer.transform, false);
    //    thisVertex.transform.position = centerRadius * Vector3.RotateTowards(Vector3.forward, Vector3.right, 30f * Mathf.Deg2Rad, 0f);


    //    Vector3[] vertices = new Vector3[12];
    //    for ( int i = 0; i < vertices.Length; i += 2 )
    //    {
    //        vertexTracer.transform.Rotate(Vector3.up, 60f);
    //        vertices[i] = thisVertex.transform.position;
    //    }
    //    for ( int i = 1; i < vertices.Length; i += 2 )
    //        vertices[i] = vertices[i - 1] - Vector3.down * 0.1f;
    //    mesh.vertices = vertices;

    //    Vector2[] uvVertices = new Vector2[12];
    //    for ( int i = 0; i < uvVertices.Length; i++ )
    //        uvVertices[i] = new Vector2((vertices[i].x + centerRadius / 2) / centerRadius, (vertices[i].y + centerRadius / 2) / centerRadius);
    //    mesh.uv = uvVertices;

    //    mesh.triangles = new int[] {
    //        0, 1, 2,    2, 1, 3,
    //        2, 3, 4,    4, 3, 5,
    //        4, 5, 6,    6, 5, 7,
    //        6, 7, 8,    8, 7, 9,
    //        8, 9, 10,   10, 9, 11,
    //        10, 11, 0,  0, 11, 1
    //    };

    //    AssetDatabase.CreateAsset(mesh, "Assets/CenterParticleMesh.asset");
    //    DestroyImmediate(thisVertex);
    //    DestroyImmediate(vertexTracer);
    //}

}
