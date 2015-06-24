using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Script Adapted from: http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels

public class CaveGeneratorScript : MonoBehaviour {


	public int[,] Map;
	
	public int MapWidth = 254;
	public int MapHeight = 254;
	public int PercentAreWalls = 25;

	public int depth = 5;
	public int bumpSize = 6;

	World world;

	bool generated = false;

	DiamondSquare diamondSquare;


	// Use this for initialization
	void Start () {
		diamondSquare = new DiamondSquare ();
	}
	
	// Update is called once per frame
	void Update () {

	
	}

	public void buildCave(int x, int y, int z){
		Map = new int[MapWidth, MapHeight];
		RandomFillMap();
		MakeCaverns ();

		float seed = Random.Range (0.0f, 1.0f);
		world = this.GetComponent<World> ();

		float[] ceil = new float[MapWidth * MapHeight];
		float[] floor = new float[MapHeight * MapWidth];

		diamondSquare.GRAIN = 15;
		diamondSquare.mountainHeight = bumpSize;
		diamondSquare.generateHeightMap (MapWidth, MapHeight, ceil);
		diamondSquare.generateHeightMap (MapWidth, MapHeight, floor);

		int floorModifier = 0;
		print (floor [0]);
		for (int i = 0; i < MapWidth; i++) {
			for(int j= 0; j < MapHeight; j++){
				if(j > 20 && j < 30 && i < 15){
					Map[i,j] = 0;
				
				}
				floorModifier = (int)Mathf.Sqrt((j-22)*(j-22) + (i+3)*(i+3))/5;

					if(Map[i,j] == 1 && (i < 5 || j < 5 || i > MapWidth -5 || j > MapHeight - 5))
					{
					for( int k = 0; k < depth+(int)(ceil[i*MapWidth + j]*bumpSize); k++)
						world.set_voxel(x + i, y+(int)(floor[i*MapWidth + j]*bumpSize)+k - (int)(floor[7*MapWidth + 22]*bumpSize) - floorModifier, z+j - 22, 3);
					}
					else if(Map[i,j] == 1){
						
					generateWeirdProp(new Vector3(x + i, y+(int)(floor[i*MapWidth + j]*bumpSize) - (int)(floor[7*MapWidth + 22]*bumpSize) - floorModifier, z+j - 22), depth+(int)(ceil[i*MapWidth + j]*bumpSize));
					}

				world.set_voxel(i + x, y+(int)(floor[i*MapWidth + j]*bumpSize)- (int)(floor[7*MapWidth + 22]*bumpSize) - floorModifier, z+ j - 22, 3);
				world.set_voxel(i+ x, y+(int)(floor[i*MapWidth + j]*bumpSize) + depth+(int)(ceil[i*MapWidth + j]*bumpSize) - (int)(floor[7*MapWidth + 22]*bumpSize) - floorModifier, z+j - 20, 3);
			}
			
		}
		print (Time.time);
	}

	void generateWeirdProp(Vector3 pos, int size){
		
		int x = Random.Range (15, 30);
		int y = Random.Range (15, 30);
		int depth = Random.Range(2, size);
		float[] heightMap = new float[depth * 360];
		diamondSquare.generateHeightMap (depth, depth , heightMap);
		float bumpSize = Random.Range (3.0f, 15.0f);
		diamondSquare.GRAIN = 6;
		

			for (int i = 0; i < depth; i++) {
				for (int j = 0; j < 360; j+= 1) {
					float rX = heightMap [j + i * depth] * bumpSize;
					float rY = heightMap [depth * i + j] * bumpSize;
				if (Random.Range (0.0f, 1.0f) > 0.5f) {
					world.set_voxel ((int)(pos.x + rX * Mathf.Cos ((j) * (Mathf.PI / 180.0f))), (int)(pos.y + i), (int)(pos.z + rY * Mathf.Sin ((j) * (Mathf.PI / 180.0f))), 3);
				}
				else{
					world.set_voxel ((int)(pos.x + rX * Mathf.Cos ((j) * (Mathf.PI / 180.0f))), (int)(pos.y +size - i), (int)(pos.z + rY * Mathf.Sin ((j) * (Mathf.PI / 180.0f))), 3);
				}
					//worldScript.set_voxel( (int)i , (int)rX, (int)j, 2);
				}
			}

		
		
	}


	public void MakeCaverns()
	{
		// By initilizing column in the outter loop, its only created ONCE
		for(int column=0, row=0; row <= MapHeight-1; row++)
		{
			for(column = 0; column <= MapWidth-1; column++)
			{
				Map[column,row] = PlaceWallLogic(column,row);
			}
		}
	}
	
	public int PlaceWallLogic(int x,int y)
	{
		int numWalls = GetAdjacentWalls(x,y,1,1);
		
		
		if(Map[x,y]==1)
		{
			if( numWalls >= 4 )
			{
				return 1;
			}
			if(numWalls<2)
			{
				return 0;
			}
			
		}
		else
		{
			if(numWalls>=5)
			{
				return 1;
			}
		}
		return 0;
	}
	
	public int GetAdjacentWalls(int x,int y,int scopeX,int scopeY)
	{
		int startX = x - scopeX;
		int startY = y - scopeY;
		int endX = x + scopeX;
		int endY = y + scopeY;
		
		int iX = startX;
		int iY = startY;
		
		int wallCounter = 0;
		
		for(iY = startY; iY <= endY; iY++) {
			for(iX = startX; iX <= endX; iX++)
			{
				if(!(iX==x && iY==y))
				{
					if(IsWall(iX,iY))
					{
						wallCounter += 1;
					}
				}
			}
		}
		return wallCounter;
	}
	
	bool IsWall(int x,int y)
	{
		// Consider out-of-bound a wall
		if( IsOutOfBounds(x,y) )
		{
			return true;
		}
		
		if( Map[x,y]==1	 )
		{
			return true;
		}
		
		if( Map[x,y]==0	 )
		{
			return false;
		}
		return false;
	}
	
	bool IsOutOfBounds(int x, int y)
	{
		if( x<0 || y<0 )
		{
			return true;
		}
		else if( x>MapWidth-1 || y>MapHeight-1 )
		{
			return true;
		}
		return false;
	}
	
	public void PrintMap()
	{
		for (int i = 0; i < MapWidth; i++) {
			for(int j= 0; j < MapHeight; j++){
				print(Map[i,j]);
			}

		}
	}
		
	public void BlankMap()
	{
		for(int column=0,row=0; row < MapHeight; row++) {
			for(column = 0; column < MapWidth; column++) {
				Map[column,row] = 0;
			}
		}
	}
	
	public void RandomFillMap()
	{
		// New, empty map
		Map = new int[MapWidth,MapHeight];
		
		int mapMiddle = 0; // Temp variable
		for(int column=0,row=0; row < MapHeight; row++) {
			for(column = 0; column < MapWidth; column++)
			{
				// If coordinants lie on the the edge of the map (creates a border)
				if(column == 0)
				{
					Map[column,row] = 1;
				}
				else if (row == 0)
				{
					Map[column,row] = 1;
				}
				else if (column == MapWidth-1)
				{
					Map[column,row] = 1;
				}
				else if (row == MapHeight-1)
				{
					Map[column,row] = 1;
				}
				// Else, fill with a wall a random percent of the time
				else
				{
					mapMiddle = (MapHeight / 2);
					
					if(row == mapMiddle)
					{
						Map[column,row] = 0;
					}
					else
					{
						Map[column,row] = RandomPercent(PercentAreWalls);
					}
				}
			}
		}
	}
	
	int RandomPercent(int percent)
	{
		if(percent>=Random.Range(1,101))
		{
			return 1;
		}
		return 0;
	}
}
