using UnityEditor;
using UnityEngine;

public class CreateInPlayMode : MonoBehaviour
{

    void Start ()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        Mesh mesh = meshFilters[0].mesh;

        AssetDatabase.CreateAsset(mesh, "Assets/GrabbedMesh.asset");


        //CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        //int i = 0;
        //while ( i < meshFilters.Length )
        //{
        //    combine[i].mesh = meshFilters[i].sharedMesh;
        //    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        //    meshFilters[i].gameObject.active = false;
        //    i++;
        //}
        //transform.GetComponent<MeshFilter>().mesh = new Mesh();
        //transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        //transform.gameObject.active = true;
        //}

        //    
    }

    // Update is called once per frame
    void Update ()
    {

    }
}
