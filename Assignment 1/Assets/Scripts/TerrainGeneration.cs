using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

public class TerrainGeneration : MonoBehaviour
{
    public int mRandomSeed;
    public int mWidth;
    public int mDepth;
    public int mMaxHeight;
    public Material mTerrainMaterial;

    //prefabs
    public GameObject firTree;
    public GameObject oakTree;
    public GameObject palmTree;
    public GameObject rock01;
    public GameObject rock02;
    public GameObject rock03;

    List<GameObject> objects = new List<GameObject>();


    private Data uvData;
    private GameObject realTerrain1;
    private GameObject realTerrain2;
    private GameObject realTerrain3;
    private GameObject realTerrain4;
    private NoiseAlgorithm terrainNoise;
    
    // code to get rid of fog from: https://forum.unity.com/threads/how-do-i-turn-off-fog-on-a-specific-camera-using-urp.1373826/
    // Unity calls this method automatically when it enables this component
    private void OnEnable()
    {
        // Add WriteLogMessage as a delegate of the RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering += BeginRender;
        RenderPipelineManager.endCameraRendering += EndRender;
    }
 
    // Unity calls this method automatically when it disables this component
    private void OnDisable()
    {
        // Remove WriteLogMessage as a delegate of the  RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering -= BeginRender;
        RenderPipelineManager.endCameraRendering -= EndRender;
    }
 
    // When this method is a delegate of RenderPipeline.beginCameraRendering event, Unity calls this method every time it raises the beginCameraRendering event
    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        // Write text to the console
        //Debug.Log($"Beginning rendering the camera: {camera.name}");
 
        if(camera.name == "Main Camera No Fog")
        {
            //Debug.Log("Turn fog off");
            RenderSettings.fog = false;
        }
         
    }
 
    void EndRender(ScriptableRenderContext context, Camera camera)
    {
        //Debug.Log($"Ending rendering the camera: {camera.name}");
        if (camera.name == "Main Camera No Fog")
        {
            //Debug.Log("Turn fog on");
            RenderSettings.fog = true;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //add prefabs to list
        objects.Add(firTree);
        objects.Add(oakTree);
        objects.Add(palmTree);
        objects.Add(rock01);
        objects.Add(rock02);
        objects.Add(rock03);
        // create a height map using perlin noise and fractal brownian motion
        terrainNoise = new NoiseAlgorithm();
        terrainNoise.InitializeNoise(mWidth + 1, mDepth + 1, mRandomSeed);
        terrainNoise.InitializePerlinNoise(1.0f, 0.5f, 8, 2.0f, 0.5f, 0.01f, 1.0f);
        NativeArray<float> terrainHeightMap = new NativeArray<float>((mWidth+1) * (mDepth+1), Allocator.Persistent);
        terrainNoise.setNoise(terrainHeightMap, 0, 0);

        // create the mesh and set it to the terrain variable
        realTerrain1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        realTerrain1.transform.position = new Vector3(0, 0, 0);
        MeshRenderer meshRenderer = realTerrain1.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = realTerrain1.GetComponent<MeshFilter>();
        meshRenderer.material = mTerrainMaterial;
        meshFilter.mesh = GenerateTerrainMesh(terrainHeightMap, new Vector2(realTerrain1.transform.position.x, realTerrain1.transform.position.z));


        // Terrain 2
        terrainNoise.setNoise(terrainHeightMap, 100, 0);

        realTerrain2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        realTerrain2.transform.position = new Vector3(100, 0, 0);
        MeshRenderer meshRenderer2 = realTerrain2.GetComponent<MeshRenderer>();
        MeshFilter meshFilter2 = realTerrain2.GetComponent<MeshFilter>();
        meshRenderer2.material = mTerrainMaterial;
        meshFilter2.mesh = GenerateTerrainMesh(terrainHeightMap, new Vector2(realTerrain2.transform.position.x, realTerrain2.transform.position.z));

        // Terrain 3
        terrainNoise.setNoise(terrainHeightMap, 100, 100);

        realTerrain3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        realTerrain3.transform.position = new Vector3(100, 0, 100);
        MeshRenderer meshRenderer3 = realTerrain3.GetComponent<MeshRenderer>();
        MeshFilter meshFilter3 = realTerrain3.GetComponent<MeshFilter>();
        meshRenderer3.material = mTerrainMaterial;
        meshFilter3.mesh = GenerateTerrainMesh(terrainHeightMap, new Vector2(realTerrain3.transform.position.x, realTerrain3.transform.position.z));

        // Terrain 4
        terrainNoise.setNoise(terrainHeightMap, 0, 100);

        realTerrain4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        realTerrain4.transform.position = new Vector3(0, 0, 100);
        MeshRenderer meshRenderer4 = realTerrain4.GetComponent<MeshRenderer>();
        MeshFilter meshFilter4 = realTerrain4.GetComponent<MeshFilter>();
        meshRenderer4.material = mTerrainMaterial;
        meshFilter4.mesh = GenerateTerrainMesh(terrainHeightMap, new Vector2(realTerrain4.transform.position.x, realTerrain4.transform.position.z));


        terrainHeightMap.Dispose();
    }

    private void Update()
    {
      
    }

    // create a new mesh with
    // perlin noise done blankly from Mathf.PerlinNoise in Unity
    // without any other features
    // makes a quad and connects it with the next quad
    // uses whatever texture the material is given
    public Mesh GenerateTerrainMesh(NativeArray<float> heightMap, Vector2 terrainCood)
    {
        int width = mWidth + 1, depth = mDepth + 1;
        int height = mMaxHeight;
        int indicesIndex = 0;
        int vertexIndex = 0;
        int vertexMultiplier = 4; // create quads to fit uv's to so we can use more than one uv (4 vertices to a quad)

        Mesh terrainMesh = new Mesh();
        List<Vector3> vert = new List<Vector3>(width * depth * vertexMultiplier);
        List<int> indices = new List<int>(width * depth * 6);
        List<Vector2> uvs = new List<Vector2>(width * depth);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (x < width - 1 && z < depth - 1)
                {
                    // note: since perlin goes up to 1.0 multiplying by a height will tend to set
                    // the average around maxheight/2. We remove most of that extra by subtracting maxheight/2
                    // so our ground isn't always way up in the air
                    float y = heightMap[(x) * (width) + (z)] * height - (mMaxHeight/2.0f);
                    float useAltXPlusY = heightMap[(x + 1) * (width) + (z)] * height - (mMaxHeight/2.0f);
                    float useAltZPlusY = heightMap[(x) * (width) + (z + 1)] * height- (mMaxHeight/2.0f);
                    float useAltXAndZPlusY = heightMap[(x + 1) * (width) + (z + 1)] * height- (mMaxHeight/2.0f);
                    
                    vert.Add(new float3(x, y, z));
                    vert.Add(new float3(x, useAltZPlusY, z + 1)); 
                    vert.Add(new float3(x + 1, useAltXPlusY, z));  
                    vert.Add(new float3(x + 1, useAltXAndZPlusY, z + 1));

                    // add uv's
                    string filepath = "./Assets/Materials/uvData.json";
                    string jsonData = System.IO.File.ReadAllText(filepath);

                    Debug.Log(jsonData);
                    uvData = new Data();

                    uvData = JsonUtility.FromJson<Data>(jsonData);
                    Debug.Log(uvData.textureUVs.Count);


                    // remember to give it all 4 sides of the image coords
                    //sand
                    if (y <= 0)
                    {
                        uvs.Add(new Vector2(uvData.textureUVs[13].pixelStartX, uvData.textureUVs[13].pixelStartY));
                        uvs.Add(new Vector2(uvData.textureUVs[13].pixelStartX, uvData.textureUVs[13].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[13].pixelEndX, uvData.textureUVs[13].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[13].pixelEndX, uvData.textureUVs[13].pixelStartY));

                        float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                        if (rand < 0.01f && y>-5)
                        {
                            Instantiate(objects[2], new Vector3(terrainCood.x + x, y, terrainCood.y + z), Quaternion.identity);
                        }
                    }
                    //grass
                    else if (y < 12)
                    {
                        uvs.Add(new Vector2(uvData.textureUVs[7].pixelStartX, uvData.textureUVs[7].pixelStartY));
                        uvs.Add(new Vector2(uvData.textureUVs[7].pixelStartX, uvData.textureUVs[7].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[7].pixelEndX, uvData.textureUVs[7].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[7].pixelEndX, uvData.textureUVs[7].pixelStartY));
                        float rand = UnityEngine.Random.Range(0.0f,1.0f);

                        if (rand < 0.01f)
                        {
                            Instantiate(objects[0], new Vector3(terrainCood.x + x, y, terrainCood.y + z), Quaternion.identity);
                        }
                        else if (rand < 0.02f)
                        {
                            Instantiate(objects[1], new Vector3(terrainCood.x + x, y, terrainCood.y + z), Quaternion.identity);
                        }
                    }
                    //stone
                    else if (y < 20)
                    {
                        uvs.Add(new Vector2(uvData.textureUVs[4].pixelStartX, uvData.textureUVs[4].pixelStartY));
                        uvs.Add(new Vector2(uvData.textureUVs[4].pixelStartX, uvData.textureUVs[4].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[4].pixelEndX, uvData.textureUVs[4].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[4].pixelEndX, uvData.textureUVs[4].pixelStartY));
                        float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                        if (rand < 0.005f)
                        {
                            Instantiate(objects[3], new Vector3(terrainCood.x + x, y, terrainCood.y + z), Quaternion.identity);
                        }
                        else if (rand < 0.01f)
                        {
                            Instantiate(objects[4], new Vector3(terrainCood.x + x, y, terrainCood.y + z), Quaternion.identity);
                        }
                        else if (rand < 0.015f)
                        {
                            Instantiate(objects[5], new Vector3(terrainCood.x + x, y, terrainCood.y + z), Quaternion.identity);
                        }
                    }
                    //snow
                    else
                    {
                        uvs.Add(new Vector2(uvData.textureUVs[15].pixelStartX, uvData.textureUVs[15].pixelStartY));
                        uvs.Add(new Vector2(uvData.textureUVs[15].pixelStartX, uvData.textureUVs[15].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[15].pixelEndX, uvData.textureUVs[15].pixelEndY));
                        uvs.Add(new Vector2(uvData.textureUVs[15].pixelEndX, uvData.textureUVs[15].pixelStartY));
                    }


                    // front or top face indices for a quad
                    //0,2,1,0,3,2
                    indices.Add(vertexIndex);
                    indices.Add(vertexIndex + 1);
                    indices.Add(vertexIndex + 2);
                    indices.Add(vertexIndex + 3);
                    indices.Add(vertexIndex + 2);
                    indices.Add(vertexIndex + 1);
                    indicesIndex += 6;
                    vertexIndex += vertexMultiplier;
                }
            }

        }
        
        // set the terrain var's for the mesh
        terrainMesh.vertices = vert.ToArray();
        terrainMesh.triangles = indices.ToArray();
        terrainMesh.SetUVs(0, uvs);
        
        // reset the mesh
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();
       
        return terrainMesh;
    }

}
