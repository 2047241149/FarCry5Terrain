using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public class Landscape : MonoBehaviour
{

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int halfWidthResolution;
    private int halfHeightResolution;
    private int realWidthResolution;
    private int realHeightResolution;
    private MeshRenderer meshRender;
    
    public Texture2D heightMap;
    public float cellSize = 1.0f;
    public float heightScale = 1.0f;
    public int widthResolution = 1024;
    public int heightResolution = 1024;
    public bool bPureWhite = false;
    public Material material;
    private Texture2D calHeightMap;
    
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

        meshRender = GetComponent<MeshRenderer>();
        if (!meshRender)
        {
            return;
        }

        RecalculateHeightsAndNormals();
        halfWidthResolution = widthResolution / 2;
        halfHeightResolution = heightResolution / 2;
        realWidthResolution = halfWidthResolution * 2;
        realHeightResolution = halfHeightResolution * 2;

        //Seutp vertices and uvs
        Vector3[] vertices = new Vector3[(realWidthResolution + 1) * (realHeightResolution + 1)];
        Vector2[] uvs = new Vector2[(realWidthResolution + 1) * (realHeightResolution + 1)];
        int index = 0;
        for (int y = -halfHeightResolution; y <= halfHeightResolution; y++)
        {
            for (int x = -halfWidthResolution; x <= halfWidthResolution; x++)
            {
                vertices[index] = new Vector3(x * cellSize, 0, y * cellSize);
                uvs[index] = new Vector2((float)(x + halfWidthResolution) / (float)realWidthResolution,
                    (float)(y + halfHeightResolution) / (float)realHeightResolution);
                index++;
            }
        }
        
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        
        // for UInt16, max index is 65535, if more than use UInt32
        mesh.indexFormat = IndexFormat.UInt32;
        
        //calculate index
        int triangleNum = realWidthResolution *  realHeightResolution * 2;
        int[] indices = new int[triangleNum * 3];
        index = 0;
        for ( int y = 0; y < realHeightResolution; y++)
        {
            for (int x = 0; x < realWidthResolution; x++)
            {

                if ((y % 2  + x % 2) == 1)
                {
                    //LeftUp triangle(00-01-10)
                    int index1 = x + y * (realWidthResolution + 1);
                    indices[index] = index1;
                    index++;
                
                    int index2 = x + (y + 1) * (realWidthResolution + 1);
                    indices[index] = index2;
                    index++;
                
                    int index3 = x + 1 + y * (realWidthResolution + 1);
                    indices[index] = index3;
                    index++;
                
                    //RightDown triangle(10-01-11)
                    int index4 = x + 1 + y * (realWidthResolution + 1);
                    indices[index] = index4;
                    index++;
                
                    int index5 = x + (y + 1) * (realWidthResolution + 1);
                    indices[index] = index5;
                    index++;
                
                    int index6 = x + 1 + (y + 1) * (realWidthResolution + 1);
                    indices[index] = index6;
                    index++;
                }
                else
                {
                    //LeftDown triangle(00-01-11)
                    int index1 = x + y * (realWidthResolution + 1);
                    indices[index] = index1;
                    index++;
                
                    int index2 = x + (y + 1) * (realWidthResolution + 1);
                    indices[index] = index2;
                    index++;
                
                    int index3 = x + 1 + (y + 1) * (realWidthResolution + 1);
                    indices[index] = index3;
                    index++;
                
                    //RightDown triangle(00-11-10)
                    int index4 = x + y * (realWidthResolution + 1);
                    indices[index] = index4;
                    index++;
                
                    int index5 = x + 1 + (y + 1) * (realWidthResolution + 1);
                    indices[index] = index5;
                    index++;
                
                    int index6 = x + 1 + y  * (realWidthResolution + 1);
                    indices[index] = index6;
                    index++;
                }
                
            }
        }
        
        mesh.SetIndices(indices.ToList(), MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshRender.material = material;
        meshRender.material.SetTexture("_HeightTex", calHeightMap);
        meshRender.material.SetFloat("_HeightScale", heightScale);
    }
    

    void RecalculateHeightsAndNormals()
    {
        if (!heightMap)
            return;
        
        Color[] heightData = heightMap.GetPixels(0);
        if (heightData.Length != heightMap.width * heightMap.height)
            return;
        
        //TODO: use RGBA8 to replace RGBA16
        calHeightMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.ARGB32, false);
        Vector3[] vertexNormals = new Vector3[heightMap.width * heightMap.height];
        for (int y = 0; y < heightMap.height - 1; y++)
        {
            for (int x = 0; x < heightMap.width - 1; x++)
            {
                // height
                int index00 = y * heightMap.width + x;
                float height00 = heightData[index00].r * heightScale;
                Vector3 vertex00 = new Vector3(x * cellSize, height00, y * cellSize);
                
                int index01 = (y + 1) * heightMap.width + x;
                float height01 =  heightData[index01].r * heightScale;
                Vector3 vertex01 = new Vector3(x * cellSize, height01, (y + 1) * cellSize);
                
                int index10 = y * heightMap.width + x + 1;
                float height10 =  heightData[index10].r * heightScale;
                Vector3 vertex10 = new Vector3((x + 1) * cellSize, height10, y * cellSize);
                
                int index11 = (y + 1) * heightMap.width + x + 1;
                float height11 =  heightData[index11].r * heightScale;
                Vector3 vertex11 = new Vector3((x + 1) * cellSize, height11, (y + 1) * cellSize);

                Vector3 faceNormal1 = math.cross(vertex01 - vertex00, vertex11 - vertex01);
                Vector3 faceNormal2 = math.cross(vertex00 - vertex10, vertex11 - vertex00);
                
                vertexNormals[index01] += faceNormal1;
                vertexNormals[index10] += faceNormal2;
                vertexNormals[index00] += faceNormal1 + faceNormal2;
                vertexNormals[index11] += faceNormal1 + faceNormal2;
            }
        }

        for (int y = 0; y < heightMap.height; y++)
        {
            for (int x = 0; x < heightMap.width; x++)
            {
                int index = y * heightMap.width + x;
                Vector3 vertexNormal = vertexNormals[index].normalized;
                float height = heightData[index].r;
                heightData[index] = new Color(height, vertexNormal.x, vertexNormal.y, vertexNormal.z);
            }
        }
        
        calHeightMap.SetPixels(heightData, 0);
        calHeightMap.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        if (meshRender)
        {
            meshRender.material.SetInt("_bPureWhite", bPureWhite ? 1 : 0);
        }
    }
}
