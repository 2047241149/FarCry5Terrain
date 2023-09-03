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
    private Vector2[] uvs;

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

    public void Init(Landscape inLandscape, IntPoint inKey, int inSectionSize, Vector3[] inVertices, int[] inIndices)
    {
        GetMeshAndMaterial();
        landscape = inLandscape;
        key = inKey;
        posX = inKey.x;
        posZ = inKey.y;
        sectionSize = inSectionSize;
        RecreateMesh(inVertices, inIndices);
    }

    void RecreateMesh(Vector3[] inVertices, int[] inIndices)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = inVertices;
        
        //calculate uvs
        uvs = new Vector2[landscape.sectionResolution * landscape.sectionResolution];
        int startVertexX = key.x * (sectionSize + 1) - key.x;
        int startVertexY = key.y * (sectionSize + 1) - key.y;
        int index = 0;
        for (int y = 0; y <= sectionSize; y++)
        {
            for (int x = 0; x <= sectionSize; x++)
            {
                int vertexX = startVertexX + x;
                int vertexY = startVertexY + y;
                index = x + y * landscape.sectionResolution;
                uvs[index] = new Vector2((float)(vertexX) / (float)(landscape.realWidthResolution - 1),
                    (float)(vertexY) / (float)(landscape.realWidthResolution - 1));
            }
        }

        mesh.uv = uvs;
        mesh.indexFormat = IndexFormat.UInt32;
        
        //TODO: material????
        mesh.SetIndices(inIndices.ToList(), MeshTopology.Triangles, 0);
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
