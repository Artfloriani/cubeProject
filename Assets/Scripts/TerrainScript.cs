using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TerrainScript : MonoBehaviour {


    //Arthur
    public int planetSize = 256;
    public int loadDistance = 32;
    public int mountainHeight = 10;
    public int groundBump = 3;
	public World worldScript;


	//Used as initial grain of the DS algorithm
	public int GRAIN=8;

	int numCubes = 0;

	bool generated = false;

	DiamondSquare diamondSquare;
	SimplexNoiseGenerator simplexNoise;
	PerlinNoise perlin;

	int diamondSquareChunk;
	
	public float frequencyY = 2.5f;
    public float frequencyX = 2.5f;
    public float persistence = 0.2f;

    public float gFrequencyX = 2.5f;
    public float gFrequencyY = 2.5f;

    public float mFrequencyX = 1;
    public float mFrequencyY = 1;
    public float mFrequencyZ = 1;
    public float mTolerance = 0.5f;
    public float mPersistence = 0.5f;

    public float mStrict = 0.6f;

    public int chunkSizeDefault = 14;

    public int iteratorCount = 3;
    //Number of samples on the vertical axis
    public int verticalIterator = 4;



    float seed = 1;



    // Use this for initialization
    void Start () {
		RenderSettings.fog = false;
		diamondSquare = new DiamondSquare ();
		diamondSquare.flat = true;

		perlin = new PerlinNoise ();



		worldScript = this.GetComponent<World> ();

		diamondSquareChunk = (int)worldScript.viewDistance*2;

        seed = Random.Range(0.5f, 0.6f);


        //generateTerrain ();

        //generateCanyon (new Vector3(300,300,300));


        print ("Total Cubes - " + numCubes);
	}
	
	// Update is called once per frame
	void Update () {
		if (!generated && Time.time >0) {
            generateTerrain2(new Vector3(0, 0, 0), false);
            generated = true;
		}
	}
	public static float lerp(float x, float x1, float x2, float q00, float q01) {
		return ((x2 - x) / (x2 - x1)) * q00 + ((x - x1) / (x2 - x1)) * q01;
	}
	
	public static float biLerp(float x, float y, float q11, float q12, float q21, float q22, float x1, float x2, float y1, float y2) {
		float r1 = lerp(x, x1, x2, q11, q21);
		float r2 = lerp(x, x1, x2, q12, q22);
		
		return lerp(y, y1, y2, r1, r2);
	}
	
	public static float triLerp(float x, float y, float z, float q000, float q001, float q010, float q011, float q100, float q101, float q110, float q111, float x1, float x2, float y1, float y2, float z1, float z2) {
		float x00 = lerp(x, x1, x2, q000, q100);
		float x10 = lerp(x, x1, x2, q010, q110);
		float x01 = lerp(x, x1, x2, q001, q101);
		float x11 = lerp(x, x1, x2, q011, q111);
		float r0 = lerp(y, y1, y2, x00, x01);
		float r1 = lerp(y, y1, y2, x10, x11);
		
		return lerp(z, z1, z2, r0, r1);
	}


	public float[,,] generateTerrain2(Vector3 pos, bool flat){

        
        float time1 = Time.time;

        //2D heightmap
		float[] heightMap;
        
        //Number of samples on the horizontal axis
		
        //loadDistance = chunkSize + 1;
     
        int chunkSize = chunkSizeDefault + iteratorCount+ chunkSizeDefault/iteratorCount;

         heightMap = new float[((chunkSize)/iteratorCount)*((chunkSize) / iteratorCount) + iteratorCount];

        float[] groundMap = new float[(chunkSize / iteratorCount) * (chunkSize / iteratorCount)];

        //Complete map including the Vertical axis
		float[,,] completeMap = new float[((chunkSize+1) + iteratorCount), mountainHeight, ((chunkSize + 1) + iteratorCount)];
       

        int maxHeight = 0;

        float min = 1;
        float max = 0;

       
        //Filling some values of the map
       
		for (int i = 0; i < chunkSize; i+= iteratorCount) {
			for (int j = 0; j< chunkSize; j+= iteratorCount) {


                //heightMap [i*loadDistance + j] = ((float)perlin.OctavePerlin((i/(float)loadDistance)*frequency, 0.5f, (j/(float)loadDistance)*frequency, 4, 0.6f));
                int indexI = i;
                int indexJ = j / (iteratorCount);
                heightMap[indexI + indexJ] = (float)perlin.UnityOctavePerlin(((i+pos.x)/(float)((planetSize + chunkSize)* seed)) *frequencyX, ((j+pos.z)/(float)((planetSize + chunkSize) * seed)) *frequencyY, 4, persistence);
            
                if (heightMap[indexI + indexJ] > max)
                {
                    max = heightMap[indexI + indexJ];
                }

                if (heightMap[indexI + indexJ] < min)
                    min = heightMap[indexI + indexJ];

                //groundMap[i * (chunkSize + 1) + j] = (float)perlin.UnityOctavePerlin(((i+pos.x) / (float)planetSize) * gFrequencyX, ((j+pos.z) / (float)planetSize) * gFrequencyY, 3, 0.2f);

                if (i == 0 || j == 0)
                {
                    heightMap[indexI + indexJ] = 0;
                    //groundMap[i * (chunkSize) + j] = 0;
                }
                int density = (int)(heightMap[indexI + indexJ] * mountainHeight);

                if (density > maxHeight)
                    maxHeight = density;
                for (int k = 0; k < density; k+= verticalIterator)
                {
                    if (perlin.OctavePerlin(((i+pos.x) / (float)(planetSize + chunkSize)) *mFrequencyX , ((k+pos.y) / (float)density) *mFrequencyZ, ((pos.z + j) / (float)(planetSize + chunkSize)) *mFrequencyY, 4, mPersistence) > mTolerance)
                    {
                        completeMap[i, k, j] = 1;

              

                    }
                }

            }
		}

         chunkSize -=  chunkSizeDefault / iteratorCount;

        for (int i = 0; i <=chunkSize; i++)
        {
            for (int j = 0; j <= chunkSize ; j++)
            {
                worldScript.set_voxel((int)pos.x + i, planetSize, (int) pos.z+j, 1);
                //used to spawn trees

                int x1 = (i - i % (iteratorCount));
                int x2 = (i + iteratorCount - (i % iteratorCount));
                int y1 = (j - j % iteratorCount);
                int y2 = (j - j % iteratorCount) + iteratorCount;



                /*
                if (x2 >= (chunkSize))
                {
                    x2 = x2 - x2 % iteratorCount +iteratorCount;
                    print(x2);
                }
                if (y1 >= chunkSize)
                    y1 = y1- y1 % iteratorCount;
                if (y2 >= chunkSize)
                    y2 = y2 - y2 % iteratorCount + iteratorCount;
                if (x1 >= chunkSize)
                    x1 = x1 - x1 % iteratorCount;*/

                for (int k = maxHeight - 1; k > 0; k--)
                {


                    int z1 = (k - k % verticalIterator);
                    int z2 = (k + verticalIterator - k % verticalIterator);


                    if (z1 >= mountainHeight)
                        z1 = mountainHeight - 1;
                    if (z2 >= mountainHeight)
                        z2 = mountainHeight - 1;

                    //INterpolate to fill all the voxel positions
                    if (completeMap[i, k, j] == 0)
                    {
                        //   print(completeMap[i, k, j]);
                        completeMap[i, k, j] = triLerp(i, k, j, completeMap[x1, z1, y1], completeMap[x1, z2, y1], completeMap[x1, z1, y2], completeMap[x1, z2, y2],
                            completeMap[x2, z1, y1], completeMap[x2, z2, y1], completeMap[x2, z1, y2], completeMap[x2, z2, y2], x1, x2, z1, z2, y1, y2);

                        // print(completeMap[i, k, j]);
                    }

                }

            }

            
        }


        /*
		for (int i = 0; i < loadDistance; i+= 1) {
			for (int j = 0; j< loadDistance; j+= 1) {

				int x1 = (i - i%(iteratorCount));
				int x2 = (i +iteratorCount - (i%iteratorCount));
				int y1 = (j-j%iteratorCount);
				int y2 = (j-j%iteratorCount) +iteratorCount;

				if(x2 >= loadDistance)
					x2 = loadDistance-1;
				if(y1 >= loadDistance)
					y1 = loadDistance-1;
				if(y2 >= loadDistance)
					y2 = loadDistance-1;
				if(x1 >=loadDistance)
					x1 = loadDistance -1;




				//INterpolate to fill all the voxel positions
				if(heightMap[i*loadDistance + j] == 0){
					heightMap [i*loadDistance + j] = biLerp(i, j, heightMap[x1*loadDistance + y1], heightMap[x1*loadDistance + y2]
					                                      , heightMap[x2*loadDistance + y1]
					                                      , heightMap[x2*loadDistance + y2]
					                                      ,x1, x2, y1, y2);
				}



				int density =  (int)(heightMap[i*loadDistance+j]*mountainHeight*heightMap[i*loadDistance+j]);
				for (int k = 1; k < density; k++) {
					if(perlin.OctavePerlin((i/(float)loadDistance)*8, (k/(float)density)/2, j/(float)loadDistance*8, 7, 0.45f) > 0.5f){
						worldScript.set_voxel (i, loadDistance + k, j, 1);
						
					}
			
				}


				worldScript.set_voxel (i, loadDistance, j, 5);
			}
		}
        */

        return completeMap;
    }


	void generateTerrainSimplex(){
		simplexNoise = new SimplexNoiseGenerator (Random.Range (0, 10000).ToString ());
		float[] heightMap;

		heightMap = new float[loadDistance * loadDistance];


		diamondSquare.GRAIN = GRAIN;
		diamondSquare.mountainHeight = mountainHeight;
		
		diamondSquare.generateHeightMap (loadDistance, loadDistance, heightMap);

		float seedX = Random.Range (5f, 10f);
		float seedY = Random.Range (1f, 10f);
	
		
		float seed = 1f;




		/*
		for (int i = 0; i < loadDistance; i+= iteratorCount) {
			for (int j = 0; j< loadDistance; j+= iteratorCount) {

				//int density = (int)(Mathf.PerlinNoise (((float)i / (float)loadDistance) * (loadDistance / 75.0f) * frequency*((i/(float)loadDistance)% (loadDistance/5.0f)), ((j / (float)loadDistance)) * (loadDistance / 75.0f)* frequency*((j/(float)loadDistance)% (loadDistance/5.0f))) * mountainHeight);
			//	int density =  (int)(heightMap[j + i*loadDistance]*mountainHeight);
				//int density = mountainHeight;
				//int density = (int)(perlin.perlin((i/(float)loadDistance), 0, (j/(float)loadDistance))*mountainHeight);
				//int density = (int)(Mathf.PerlinNoise((i/(float)loadDistance)* (loadDistance / 75.0f)*frequency, (j/(float)loadDistance)* (loadDistance / 75.0f)*frequency)*mountainHeight);
				int density = (int)(perlin.OctavePerlin((i/(float)loadDistance)*frequency, 0, (j/(float)loadDistance)*frequency, 4, 0.5f)*mountainHeight);
				for (int k = 1; k < density; k++) {
					//worldScript.set_voxel (i, loadDistance + k, j, 1);
					if(perlin.OctavePerlin((i/(float)loadDistance)*4, k/(float)density, j/(float)loadDistance*4, 8, 0.3f) > 0.5f){
					//if (simplexNoise.noise ((i / (float)loadDistance)  , (k / (float)density)*5, (j / (float)loadDistance))  > 0.0f) {
						worldScript.set_voxel (i, loadDistance + k, j, 1);

					}



				}
				worldScript.set_voxel (i, loadDistance, j, 5);
			}
		}
		*/
	}
	

	
	void generateTerrain()
	{
		float[] heightMap;
		float[] treeMap;

		heightMap = new float[loadDistance * loadDistance];
		treeMap = new float[loadDistance * loadDistance];

		diamondSquare.GRAIN = GRAIN;
		diamondSquare.mountainHeight = mountainHeight;

		diamondSquare.generateHeightMap(loadDistance,loadDistance, heightMap);

		diamondSquare.GRAIN = 15;
		diamondSquare.mountainHeight = 35;
		diamondSquare.generateHeightMap(loadDistance,loadDistance, treeMap);
		//Top Plane
		int mountainX = Random.Range (0, loadDistance); 
		int mountainY = Random.Range (0, loadDistance);
		int numX = Random.Range(30, 60);
		int numY =  Random.Range(30, 60);
		int depth = Random.Range (15, 20);


		for (int i = 0; i < loadDistance; i++) {
			for(int j = 0; j< loadDistance; j++){
				
				//int height = (int)(mountainHeight*Mathf.PerlinNoise ( (float)seed * i, (float)seed * (j)));
				int height = (int)(mountainHeight*heightMap[j + i*loadDistance]);
				numCubes++;

				float dx = i - loadDistance/2;
				float dy =  j - loadDistance/2;
				if(( dx * dx ) / ( numX * numX ) + ( dy * dy ) / ( numY * numY ) > 0.95f){
					worldScript.set_voxel(i, loadDistance+ height,  j, 1);
					if(Random.Range(1, (int)(300/(float)treeMap[j + i*loadDistance])) == 25 && i > 30 && j > 30 && i < loadDistance - 30 && j < loadDistance -30 && height < mountainHeight*0.8f)
					{
						this.GetComponent<TreeGeneratorScript>().generateTree(new Vector3(i, loadDistance + height, j));
					}
				}

				if(i == loadDistance/2 && j == loadDistance/2)
				{
					generateCanyon (new Vector3 (loadDistance/2, loadDistance - depth, loadDistance/2), numX, numY, depth, heightMap);
				}




			}
		}

	

		/*

		diamondSquare.generateHeightMap(loadDistance,loadDistance, heightMap);
		//Down Plane
		for (int i = 0; i < loadDistance; i++) {
			for(int j = 0; j< loadDistance; j++){
				//int height = (int)(mountainHeight*Mathf.PerlinNoise ( (float)seed * i, (float)seed * (j)));
				
				int height = (int)(heightMap[i + (loadDistance)*j]*mountainHeight);
				
				numCubes++;
				worldScript.set_voxel(i, height,  j, 1);
			}
		}


		diamondSquare.mountainHeight = 35;
		diamondSquare.generateHeightMap(loadDistance,loadDistance, heightMap);
		//Left Plane
		for (int i = 0; i < loadDistance; i++) {
			for(int j = 0; j< loadDistance; j++){
				GRAIN = 12;
				int height = (int)(mountainHeight*heightMap[j + i*loadDistance]);
				numCubes++;
				worldScript.set_voxel(height, i,  j, 4);
			}
		}



		diamondSquare.generateHeightMap(loadDistance,loadDistance, heightMap);
		//Right Plane
		for (int i = 0; i < loadDistance; i++) {
			for(int j = 0; j< loadDistance; j++){

				int height = (int)(mountainHeight*heightMap[j + i*loadDistance]) ;
				numCubes++;
				worldScript.set_voxel(loadDistance + height, i,  j, 1);
			}
		}
		diamondSquare.generateHeightMap(loadDistance,loadDistance, heightMap);
		//Front Plane
		for (int i = 0; i < loadDistance; i++) {
			for(int j = 0; j< loadDistance; j++){
				
				int height = (int)(mountainHeight*heightMap[j + i*loadDistance]);
				numCubes++;
				worldScript.set_voxel(i, j,  height, 3);
			}
		}
		diamondSquare.generateHeightMap(loadDistance,loadDistance, heightMap);
		//Back Plane
		for (int i = 0; i < loadDistance; i++) {
			for(int j = 0; j< loadDistance; j++){
				
				int height = (int)(mountainHeight*heightMap[j + i*loadDistance]);
				numCubes++;
				worldScript.set_voxel(i, j,  loadDistance+height, 3);
			}
		}

		*/

	}

	void generateCanyon(Vector3 pos, int x, int y, int depth, float[] map){
		
		float[] heightMap = new float[360*360];
		diamondSquare.generateHeightMap (360, 360, heightMap);
	
		float bumpSize = 5.0f;
		diamondSquare.GRAIN = 5;




		for(int j = 0; j < 360; j+= 1){
			for (int i = 0; i < depth + mountainHeight*map[(int)((int)(pos.x+x*Mathf.Cos ((j) * (Mathf.PI / 180.0f)))*loadDistance + (int)(pos.z+y*Mathf.Sin ((j) * (Mathf.PI / 180.0f))))]; i++) {

				if(j < 15 && i < depth/2){
				}else{
					float rX = x + heightMap[i + j*360]* bumpSize;
					float rY = y + heightMap[x*j +i]* bumpSize;
					worldScript.set_voxel(  (int)(pos.x+rX*Mathf.Cos ((j) * (Mathf.PI / 180.0f))), (int) (pos.y+ i), (int)(pos.z+rY*Mathf.Sin ((j) * (Mathf.PI / 180.0f))), 3);
				}

			}
		}

		//Generating Cave
		CaveGeneratorScript cave =  this.GetComponent<CaveGeneratorScript> ();
		float cX = x + heightMap[0]* bumpSize;
		float cY = y + heightMap[0]* bumpSize;
		cave.buildCave ((int)(pos.x + cX * Mathf.Cos ((0) * (Mathf.PI / 180.0f))), (int)(pos.y + 0), (int)(pos.z + cY * Mathf.Sin ((0) * (Mathf.PI / 180.0f))));
		
		/*
		for(int j = 0; j < 360; j+= 1){
			float rX = x - heightMap[j + (depth-1)*x]* bumpSize;
			float rY = y - heightMap[x*(depth-1) +j]* bumpSize;
			for(int k = depth; k < mountainHeight*map[(int)((int)(pos.x+rX*Mathf.Cos ((j) * (Mathf.PI / 180.0f)))*loadDistance + (int)(pos.z+rY*Mathf.Sin ((j) * (Mathf.PI / 180.0f))))]; k++){
				worldScript.set_voxel(  (int)(pos.x+rX*Mathf.Cos ((j) * (Mathf.PI / 180.0f))), (int) (pos.y+ (depth-1)+k), (int)(pos.z+rY*Mathf.Sin ((j) * (Mathf.PI / 180.0f))), 2);
					
			}
		}*/


		diamondSquare.generateHeightMap (x, x, heightMap);
		
		bumpSize = 8.0f;
		diamondSquare.GRAIN = 2;

		for(int i = 0; i < x; i++){
			for(int j = 0; j < y; j++)
			{
				worldScript.set_voxel((int)(pos.x +i), (int)(pos.y+heightMap[i*x +j]*bumpSize), (int)(pos.z + j), 2);
				worldScript.set_voxel((int)(pos.x -i), (int)(pos.y+heightMap[i*x +j]*bumpSize), (int)(pos.z + j), 2);
				worldScript.set_voxel((int)(pos.x -i), (int)(pos.y+heightMap[i*x +j]*bumpSize), (int)(pos.z - j), 2);
				worldScript.set_voxel((int)(pos.x +i), (int)(pos.y+heightMap[i*x +j]*bumpSize), (int)(pos.z - j), 2);
			}
		}



		generateWeirdProp (pos);

	}


	void generateWeirdProp(Vector3 pos){
		
		int x = Random.Range (15, 30);
		int y = Random.Range (15, 30);
		int depth = Random.Range (20, 50);
		float[] heightMap = new float[depth * 360];
		diamondSquare.generateHeightMap (depth, depth , heightMap);
		float bumpSize = Random.Range (5.0f, 10.0f);
		diamondSquare.GRAIN = 6;
		
		
		for (int i = 0; i < depth; i++) {
			for(int j = 0; j < 360; j+= 5){
				float rX = heightMap[j + i*depth]* bumpSize;
				float rY = heightMap[depth*i +j]* bumpSize;
				worldScript.set_voxel(  (int)(pos.x+rX*Mathf.Cos ((j) * (Mathf.PI / 180.0f))), (int) (pos.y+ i), (int)(pos.z+rY*Mathf.Sin ((j) * (Mathf.PI / 180.0f))), 5);
				//worldScript.set_voxel( (int)i , (int)rX, (int)j, 2);
			}
		}
		
		
	}

	

	

	
}
