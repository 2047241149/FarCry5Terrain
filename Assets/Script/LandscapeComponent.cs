using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

public class LandscapeComponent : MonoBehaviour
{
    [VisibleOnly]
    public IntPoint key;

    [VisibleOnly] public int posX;
    [VisibleOnly] public int posZ;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int sectionSize;
    private Landscape landscape;
    private Material _material;

    // Start is called before the first frame update

    void GetMeshAndMaterial()
    {
        if(!meshFilter)
            meshFilter = GetComponent<MeshFilter>();
        
        if(!meshRenderer)
            meshRenderer = GetComponent<MeshRenderer>();
        
        if(!_material)
            _material = new Material(Shader.Find("Landscape/RenderLandscape"));
    }

    public void Init(Landscape inLandscape, IntPoint inKey, int inSectionSize, Vector3[] inVertices, Vector2[] inUVs)
    {
        GetMeshAndMaterial();
        landscape = inLandscape;
        key = inKey;
        posX = inKey.x;
        posZ = inKey.y;
        sectionSize = inSectionSize;
        RecreateMesh(inVertices, inUVs);
    }

    void RecreateMesh(Vector3[] inVertices, Vector2[] inUVs)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = inVertices;
        mesh.uv = inUVs;
        
        //calculate index
        int triangleNum = sectionSize *  sectionSize * 2;
        int[] indices = new int[triangleNum * 3];

        int startVertexX = key.x * (sectionSize + 1) - key.x;
        int startVertexY = key.y * (sectionSize + 1) - key.y;
        
        mesh.indexFormat = IndexFormat.UInt32;

        int index = 0;
        for (int y = 0; y < sectionSize; y++)
        {
            for (int x = 0; x < sectionSize; x++)
            {
                int vertexX = startVertexX + x;
                int vertexY = startVertexY + y;
                
                if ((vertexY % 2  + vertexX % 2) == 1)
                {
                    //LeftUp triangle(00-01-10)
                    int index1 = vertexX + vertexY * landscape.realWidthResolution;
                    indices[index] = index1;
                    index++;
                
                    int index2 = vertexX + (vertexY + 1) * landscape.realWidthResolution;
                    indices[index] = index2;
                    index++;
                
                    int index3 = vertexX + 1 + vertexY * landscape.realWidthResolution;
                    indices[index] = index3;
                    index++;
                
                    //RightDown triangle(10-01-11)
                    int index4 = vertexX + 1 + vertexY * landscape.realWidthResolution;
                    indices[index] = index4;
                    index++;
                
                    int index5 = vertexX + (vertexY + 1) * landscape.realWidthResolution;
                    indices[index] = index5;
                    index++;
                
                    int index6 = vertexX + 1 + (vertexY + 1) * landscape.realWidthResolution;
                    indices[index] = index6;
                    index++;
                }
                else
                {
                    //LeftDown triangle(00-01-11)
                    int index1 = vertexX + vertexY * landscape.realWidthResolution;
                    indices[index] = index1;
                    index++;
                
                    int index2 = vertexX + (vertexY + 1) * landscape.realWidthResolution;
                    indices[index] = index2;
                    index++;
                
                    int index3 = vertexX + 1 + (vertexY + 1) * landscape.realWidthResolution;
                    indices[index] = index3;
                    index++;
                
                    //RightDown triangle(00-11-10)
                    int index4 = vertexX + vertexY * landscape.realWidthResolution;
                    indices[index] = index4;
                    index++;
                
                    int index5 = vertexX + 1 + (vertexY + 1) * landscape.realWidthResolution;
                    indices[index] = index5;
                    index++;
                
                    int index6 = vertexX + 1 + vertexY  * landscape.realWidthResolution;
                    indices[index] = index6;
                    index++;
                }
            }
        }
        
        //TODO: material????
        mesh.SetIndices(indices.ToList(), MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshRenderer.material = _material;
        meshRenderer.material.SetTexture("_HeightTex", landscape.calHeightMap);
        meshRenderer.material.SetFloat("_HeightScale", landscape.heightScale);
    }

    protected void Update()
    {
        if (landscape && meshRenderer && meshRenderer.material)
        {
            meshRenderer.material.SetInt("_bPureWhite", landscape.bPureWhite ? 1 : 0);
        }
    }
}
