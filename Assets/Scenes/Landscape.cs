using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Landscape : MonoBehaviour
{

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int halfWidthResolution;
    private int halfHeightResolution;
    private int realWidthResolution;
    private int realHeightResolution;
    
    public Texture2D heightMap;
    public float size = 1.0f;
    public int widthResolution = 1024;
    public int heightResolution = 1024;
    public Material material;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        Mesh mesh = new Mesh();
        if (!mesh)
        {
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            return;
        }

        MeshRenderer meshRender = GetComponent<MeshRenderer>();
        if (!meshRender)
        {
            return;
        }
        
        
        halfWidthResolution = widthResolution / 2;
        halfHeightResolution = heightResolution / 2;
        realWidthResolution = halfWidthResolution * 2;
        realHeightResolution = halfHeightResolution * 2;

        //Seutp vertices and uvs
        Vector3[] vertices = new Vector3[(realWidthResolution + 1) * (realHeightResolution + 1)];
        Vector2[] uvs = new Vector2[(realWidthResolution + 1) * (realHeightResolution + 1)];
        int index = 0;
        for (int x = -halfWidthResolution; x <= halfWidthResolution; x++)
        {
            for (int y = -halfHeightResolution; y <= halfHeightResolution; y++)
            {
                vertices[index] = new Vector3(x * size, 0, y * size);
                uvs[index] = new Vector2((float)(x + halfWidthResolution) / (float)realWidthResolution,
                    (float)(y + halfHeightResolution) / (float)realHeightResolution);
                index++;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        
        //calculate index
        int triangleNum = vertices.Length * 2;
        int[] indices = new int[triangleNum * 3];
        index = 0;
        for (int x = 0; x < realWidthResolution; x++)
        {
            for (int y = 0; y < realHeightResolution; y++)
            {
                //LeftUp triangle
                int index1 = x + y * realWidthResolution;
                indices[index] = index1;
                index++;
                
                int index2 = x + (y + 1) * realWidthResolution;
                indices[index] = index2;
                index++;
                
                int index3 = x + 1 + y * realWidthResolution;
                indices[index] = index3;
                index++;
                
                //LeftUp triangle
                int index4 = x + 1 + y * realWidthResolution;
                indices[index] = index4;
                index++;
                
                int index5 = x + (y + 1) * realWidthResolution;
                indices[index] = index5;
                index++;
                
                int index6 = x + 1 + (y + 1) * realWidthResolution;
                indices[index] = index6;
                index++;
            }
        }
        
        mesh.SetIndices(indices.ToList(), MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshRender.material = material;
        meshRender.material.SetTexture("HeightMap", heightMap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
