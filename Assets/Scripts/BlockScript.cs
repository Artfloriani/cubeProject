using UnityEngine;
using System.Collections;

public class BlockScript {

    private float[,,] loadVoxels;
    int maxHeight;

    float x, y, z;
    int chunkSize = 14;
    float mThreashold = 0.6f;
    public bool loaded = false;
    public bool unload = false;
    World worldScript;
    TerrainScript terrainScript;


    // Use this for initialization
    public void setup () {
        worldScript = GameObject.Find("World").GetComponent<World>();
        terrainScript = GameObject.Find("World").GetComponent<TerrainScript>();
        chunkSize = terrainScript.chunkSizeDefault;
        y = terrainScript.planetSize;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void loadChunk(float xVal, float yVal, float zVal)
    {
        x = xVal;
        z = zVal;

        x = x - x % (chunkSize);
        z = z - z % (chunkSize);
        y = y - y % (chunkSize);

        
        
        Vector3 position = new Vector3(x, 0, z);
        maxHeight = terrainScript.mountainHeight;
        loadVoxels = terrainScript.generateTerrain2(position, true);
    }
    

    public void displayChunk()
    {
        
        for (int i = 0; i <= chunkSize +2; i++)
        {
            for(int j = 0; j <= chunkSize + 2; j++)
            {
                for(int k = 0; k < maxHeight; k++)
                {
                    if (Mathf.Abs(loadVoxels[i, k, j]) > mThreashold)
                    {
                        if (Mathf.Abs(loadVoxels[i, k + 1, j]) > mThreashold)
                            worldScript.set_voxel(i + (int) x, (int)y + k, (int)z + j, 3);
                        else
                        {
                            worldScript.set_voxel(i + (int)x, (int)y + k, (int)z + j, 1);

                        }
                    }
                }
            }
        }
        loaded = true;
    }


}
