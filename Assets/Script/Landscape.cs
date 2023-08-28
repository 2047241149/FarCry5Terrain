using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;



public class Landscape : MonoBehaviour
{
    
    private int halfWidthResolution;
    private int halfHeightResolution;
    [DoNotSerialize, VisibleOnly]
    public int realWidthResolution;
    private int realHeightResolution;
    private GameObject landscapeComponentPrefab;
    private Dictionary<IntPoint, GameObject> landscomponents;
    private Vector3[] vertices = null;
    private Vector2[] uvs = null;
        
    public Texture2D heightMap;
    
    public int LandscapeComponentNumX = 1;
    public int LandscapeComponentNumZ = 1;
    
    [VisibleOnly]
    public int sectionSize = 128;
    public float cellSize = 1.0f;
    public float heightScale = 1.0f;
    public bool bPureWhite = false;

    [DoNotSerialize, VisibleOnly]
    public Texture2D calHeightMap;
    
    // Start is called before the first frame update
    void Start()
    {
        landscomponents = new Dictionary<IntPoint, GameObject>();
        landscapeComponentPrefab = (GameObject)Resources.Load("Prefabs/LandscapeComponent");
        if (!landscapeComponentPrefab)
        {
            Debug.LogError("landscapeComponentPrefab is null");
            return;
        }
        
        RecalculateHeightsAndNormals();
        realWidthResolution = (sectionSize + 1) * LandscapeComponentNumX - LandscapeComponentNumX + 1;
        realHeightResolution = (sectionSize + 1) * LandscapeComponentNumZ - LandscapeComponentNumZ + 1;
        halfWidthResolution = realWidthResolution / 2;
        halfHeightResolution = realHeightResolution / 2;
        
        //Seutp vertices and uvs
        vertices = new Vector3[realWidthResolution * realHeightResolution];
        uvs = new Vector2[realWidthResolution * realHeightResolution];
        int index = 0;
        for (int y = -halfHeightResolution; y <= halfHeightResolution; y++)
        {
            for (int x = -halfWidthResolution; x <= halfWidthResolution; x++)
            {
                vertices[index] = new Vector3(x * cellSize, 0, y * cellSize);
                uvs[index] = new Vector2((float)(x + halfWidthResolution) / (float)(realWidthResolution - 1),
                    (float)(y + halfHeightResolution) / (float)(realWidthResolution - 1));
                
                index++;
            }
        }
        
        for (int x = 0; x < LandscapeComponentNumX; x++)
        {
            for (int y = 0; y < LandscapeComponentNumZ; y++)
            {
                GameObject landscapeComponentObject =  Instantiate<GameObject>(landscapeComponentPrefab, transform.position, transform.rotation);
                if (landscapeComponentObject)
                {
                    landscapeComponentObject.transform.SetParent(transform);
                    IntPoint componentKey = new IntPoint(x, y);
                    if (!landscomponents.ContainsKey(componentKey))
                    {
                        landscomponents.Add(componentKey, landscapeComponentObject);
                        LandscapeComponent landscapeComponent =
                            landscapeComponentObject.GetComponent<LandscapeComponent>();
                        if (landscapeComponent)
                        {
                            landscapeComponent.Init(this, componentKey, sectionSize, vertices, uvs);
                        }
                    }
                }
            }
        }
        
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
}
