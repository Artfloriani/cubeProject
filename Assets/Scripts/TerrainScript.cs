using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TerrainScript : MonoBehaviour {


	//Arthur
	public int planetSize = 256;
	public int mountainHeight = 10;
	public World worldScript;


	//Used as initial grain of the DS algorithm
	public int GRAIN=8;

	int numCubes = 0;

	bool generated = false;

	DiamondSquare diamondSquare;
	SimplexNoiseGenerator simplexNoise;
	PerlinNoise perlin;

	int diamondSquareChunk;
	
	public float frequency = 2.5f;

	// Use this for initialization
	void Start () {
		RenderSettings.fog = false;
		diamondSquare = new DiamondSquare ();
		diamondSquare.flat = true;

		perlin = new PerlinNoise ();



		worldScript = this.GetComponent<World> ();

		diamondSquareChunk = (int)worldScript.viewDistance*2;


		//generateTerrain ();

		//generateCanyon (new Vector3(300,300,300));


		print ("Total Cubes - " + numCubes);
	}
	
	// Update is called once per frame
	void Update () {
		if (!generated) {
			generateTerrain2 ();
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


	void generateTerrain2(){
		float[] heightMap;

		int chunkSize = 16;
		int iteratorCount = (chunkSize * chunkSize) / 32;
		heightMap = new float[(planetSize * planetSize)];

		//float[,,] completeMap = new float[planetSize, mountainHeight, planetSize];
	
		//Filling some values of the terrain height
		for (int i = 0; i < planetSize; i+= iteratorCount) {
			for (int j = 0; j< planetSize; j+= iteratorCount) {
				
				//heightMap [i*planetSize + j] = ((float)perlin.OctavePerlin((i/(float)planetSize)*frequency, 0.5f, (j/(float)planetSize)*frequency, 4, 0.6f));
				heightMap[i*planetSize +j] = (float)perlin.UnityOctavePerlin((i/(float)planetSize)*frequency, (j/(float)planetSize)*frequency, 8, 0.5f);

			}
		}

		for(int j = 0; j< planetSize; j+=iteratorCount)
			heightMap[(planetSize-1)*planetSize +j] = ((float)perlin.OctavePerlin(((planetSize-1)/(float)planetSize), 0, (j/(float)planetSize), 8, 0.7f));

		for(int i = 0; i< planetSize; i+= iteratorCount)
			heightMap[i*planetSize +planetSize-1] = ((float)perlin.OctavePerlin(((i)/(float)planetSize), 0, ((planetSize-1)/(float)planetSize), 4, 0.5f));

		heightMap[(planetSize-1)*planetSize +planetSize-1] = ((float)perlin.OctavePerlin((planetSize-1)/(float)planetSize, 0, ((planetSize-1)/(float)planetSize), 4, 0.5f));

		for (int i = 0; i < planetSize; i+= 1) {
			for (int j = 0; j< planetSize; j+= 1) {

				int x1 = (i - i%(iteratorCount));
				int x2 = (i +iteratorCount - (i%iteratorCount));
				int y1 = (j-j%iteratorCount);
				int y2 = (j-j%iteratorCount) +iteratorCount;

				if(x2 >= planetSize)
					x2 = planetSize-1;
				if(y1 >= planetSize)
					y1 = planetSize-1;
				if(y2 >= planetSize)
					y2 = planetSize-1;
				if(x1 >=planetSize)
					x1 = planetSize -1;




				//INterpolate to fill all the voxel positions
				if(heightMap[i*planetSize + j] == 0){
					heightMap [i*planetSize + j] = biLerp(i, j, heightMap[x1*planetSize + y1], heightMap[x1*planetSize + y2]
					                                      , heightMap[x2*planetSize + y1]
					                                      , heightMap[x2*planetSize + y2]
					                                      ,x1, x2, y1, y2);
				}



				int density =  (int)(heightMap[i*planetSize+j]*mountainHeight*heightMap[i*planetSize+j]);
				for (int k = 1; k < density; k++) {
					if(perlin.OctavePerlin((i/(float)planetSize)*8, (k/(float)density)/2, j/(float)planetSize*8, 7, 0.45f) > 0.5f){
						//if (simplexNoise.noise ((i / (float)planetSize)  , (k / (float)density)*5, (j / (float)planetSize))  > 0.0f) {
						worldScript.set_voxel (i, planetSize + k, j, 1);
						
					}
			
				}


				worldScript.set_voxel (i, planetSize, j, 5);
			}
		}



	}


	void generateTerrainSimplex(){
		simplexNoise = new SimplexNoiseGenerator (Random.Range (0, 10000).ToString ());
		float[] heightMap;

		heightMap = new float[planetSize * planetSize];


		diamondSquare.GRAIN = GRAIN;
		diamondSquare.mountainHeight = mountainHeight;
		
		diamondSquare.generateHeightMap (planetSize, planetSize, heightMap);

		float seedX = Random.Range (5f, 10f);
		float seedY = Random.Range (1f, 10f);
	
		
		float seed = 1f;




		/*
		for (int i = 0; i < planetSize; i+= iteratorCount) {
			for (int j = 0; j< planetSize; j+= iteratorCount) {

				//int density = (int)(Mathf.PerlinNoise (((float)i / (float)planetSize) * (planetSize / 75.0f) * frequency*((i/(float)planetSize)% (planetSize/5.0f)), ((j / (float)planetSize)) * (planetSize / 75.0f)* frequency*((j/(float)planetSize)% (planetSize/5.0f))) * mountainHeight);
			//	int density =  (int)(heightMap[j + i*planetSize]*mountainHeight);
				//int density = mountainHeight;
				//int density = (int)(perlin.perlin((i/(float)planetSize), 0, (j/(float)planetSize))*mountainHeight);
				//int density = (int)(Mathf.PerlinNoise((i/(float)planetSize)* (planetSize / 75.0f)*frequency, (j/(float)planetSize)* (planetSize / 75.0f)*frequency)*mountainHeight);
				int density = (int)(perlin.OctavePerlin((i/(float)planetSize)*frequency, 0, (j/(float)planetSize)*frequency, 4, 0.5f)*mountainHeight);
				for (int k = 1; k < density; k++) {
					//worldScript.set_voxel (i, planetSize + k, j, 1);
					if(perlin.OctavePerlin((i/(float)planetSize)*4, k/(float)density, j/(float)planetSize*4, 8, 0.3f) > 0.5f){
					//if (simplexNoise.noise ((i / (float)planetSize)  , (k / (float)density)*5, (j / (float)planetSize))  > 0.0f) {
						worldScript.set_voxel (i, planetSize + k, j, 1);

					}



				}
				worldScript.set_voxel (i, planetSize, j, 5);
			}
		}
		*/
	}
	

	
	void generateTerrain()
	{
		float[] heightMap;
		float[] treeMap;

		heightMap = new float[planetSize * planetSize];
		treeMap = new float[planetSize * planetSize];

		diamondSquare.GRAIN = GRAIN;
		diamondSquare.mountainHeight = mountainHeight;

		diamondSquare.generateHeightMap(planetSize,planetSize, heightMap);

		diamondSquare.GRAIN = 15;
		diamondSquare.mountainHeight = 35;
		diamondSquare.generateHeightMap(planetSize,planetSize, treeMap);
		//Top Plane
		int mountainX = Random.Range (0, planetSize); 
		int mountainY = Random.Range (0, planetSize);
		int numX = Random.Range(30, 60);
		int numY =  Random.Range(30, 60);
		int depth = Random.Range (15, 20);


		for (int i = 0; i < planetSize; i++) {
			for(int j = 0; j< planetSize; j++){
				
				//int height = (int)(mountainHeight*Mathf.PerlinNoise ( (float)seed * i, (float)seed * (j)));
				int height = (int)(mountainHeight*heightMap[j + i*planetSize]);
				numCubes++;

				float dx = i - planetSize/2;
				float dy =  j - planetSize/2;
				if(( dx * dx ) / ( numX * numX ) + ( dy * dy ) / ( numY * numY ) > 0.95f){
					worldScript.set_voxel(i, planetSize+ height,  j, 1);
					if(Random.Range(1, (int)(300/(float)treeMap[j + i*planetSize])) == 25 && i > 30 && j > 30 && i < planetSize - 30 && j < planetSize -30 && height < mountainHeight*0.8f)
					{
						this.GetComponent<TreeGeneratorScript>().generateTree(new Vector3(i, planetSize + height, j));
					}
				}

				if(i == planetSize/2 && j == planetSize/2)
				{
					generateCanyon (new Vector3 (planetSize/2, planetSize - depth, planetSize/2), numX, numY, depth, heightMap);
				}




			}
		}

	

		/*

		diamondSquare.generateHeightMap(planetSize,planetSize, heightMap);
		//Down Plane
		for (int i = 0; i < planetSize; i++) {
			for(int j = 0; j< planetSize; j++){
				//int height = (int)(mountainHeight*Mathf.PerlinNoise ( (float)seed * i, (float)seed * (j)));
				
				int height = (int)(heightMap[i + (planetSize)*j]*mountainHeight);
				
				numCubes++;
				worldScript.set_voxel(i, height,  j, 1);
			}
		}


		diamondSquare.mountainHeight = 35;
		diamondSquare.generateHeightMap(planetSize,planetSize, heightMap);
		//Left Plane
		for (int i = 0; i < planetSize; i++) {
			for(int j = 0; j< planetSize; j++){
				GRAIN = 12;
				int height = (int)(mountainHeight*heightMap[j + i*planetSize]);
				numCubes++;
				worldScript.set_voxel(height, i,  j, 4);
			}
		}



		diamondSquare.generateHeightMap(planetSize,planetSize, heightMap);
		//Right Plane
		for (int i = 0; i < planetSize; i++) {
			for(int j = 0; j< planetSize; j++){

				int height = (int)(mountainHeight*heightMap[j + i*planetSize]) ;
				numCubes++;
				worldScript.set_voxel(planetSize + height, i,  j, 1);
			}
		}
		diamondSquare.generateHeightMap(planetSize,planetSize, heightMap);
		//Front Plane
		for (int i = 0; i < planetSize; i++) {
			for(int j = 0; j< planetSize; j++){
				
				int height = (int)(mountainHeight*heightMap[j + i*planetSize]);
				numCubes++;
				worldScript.set_voxel(i, j,  height, 3);
			}
		}
		diamondSquare.generateHeightMap(planetSize,planetSize, heightMap);
		//Back Plane
		for (int i = 0; i < planetSize; i++) {
			for(int j = 0; j< planetSize; j++){
				
				int height = (int)(mountainHeight*heightMap[j + i*planetSize]);
				numCubes++;
				worldScript.set_voxel(i, j,  planetSize+height, 3);
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
			for (int i = 0; i < depth + mountainHeight*map[(int)((int)(pos.x+x*Mathf.Cos ((j) * (Mathf.PI / 180.0f)))*planetSize + (int)(pos.z+y*Mathf.Sin ((j) * (Mathf.PI / 180.0f))))]; i++) {

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
			for(int k = depth; k < mountainHeight*map[(int)((int)(pos.x+rX*Mathf.Cos ((j) * (Mathf.PI / 180.0f)))*planetSize + (int)(pos.z+rY*Mathf.Sin ((j) * (Mathf.PI / 180.0f))))]; k++){
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
