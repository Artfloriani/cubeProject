using UnityEngine;
using System.Collections;

public class DiamondSquare {

	float pwidthpheight;
	int pwdith;
	int pheight;
	public int GRAIN=8;
	public int mountainHeight = 10;
	public bool flat = false;


	int dummy = 0;
	
	int numCubes = 0;
	
	bool generated = false;




	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//	--- actual code starts ----
	//CODE MODIFIED FROM http://unitycoder.com/blog/2012/04/03/diamond-square-algorithm/
	float displace(float num)
	{
		//			if (dummy<300) {print(num); dummy++;}
		//			Debug.Break();
		float max = (num / pwidthpheight) * GRAIN;
		return Random.Range(-0.5f, 0.5f)* max;
	}
	
	//This is something of a "helper function" to create an initial grid
	//before the recursive function is called. 
	public void generateHeightMap(float h, float w, float[] heights)
	{
		float c1, c2, c3, c4;
		pwidthpheight= (float)h+w;
		
		pwdith = (int)w;
		pheight = (int)h;
		
		
		//Assign the four corners of the intial grid random color values
		//These will end up being the colors of the four corners of the applet.     
		c1 = Random.value;
		c2 = Random.value;
		c3 = Random.value;
		c4 = Random.value;
		
		
		divideGrid(0.0f, 0.0f, w , h , c1, c2, c3, c4, heights);
	}
	
	//This is the recursive function that implements the random midpoint
	//displacement algorithm.  It will call itself until the grid pieces
	//become smaller than one pixel.   
	void divideGrid(float x, float y, float w, float h, float c1, float c2, float c3, float c4, float[] heights)
	{
		
		
		
		float newWidth = w * 0.5f;
		float newHeight = h * 0.5f;
		
		if (w < 1.0f || h < 1.0f)
		{
			//The four corners of the grid piece will be averaged and drawn as a single pixel.
			float c = (c1 + c2 + c3 + c4) * 0.25f;
			heights[(int)x+(int)y*pwdith] = c;
		}
		else
		{
			float middle =(c1 + c2 + c3 + c4) * 0.25f + displace(newWidth + newHeight);      //Randomly displace the midpoint!
			float edge1 = (c1 + c2) * 0.5f; //Calculate the edges by averaging the two corners of each edge.
			float edge2 = (c2 + c3) * 0.5f;
			float edge3 = (c3 + c4) * 0.5f;
			float edge4 = (c4 + c1) * 0.5f;
			
			//Make sure that the midpoint doesn't accidentally "randomly displaced" past the boundaries!
			if (middle <= 0)
			{
				middle = 0;
			}
			else if (middle > 1.0f)
			{
				middle = 1.0f;
			}


			if(flat){
				if (x == 0) {
					c1 = c4 = 0;
					
				}
				if(y == 0){
					c1 = c2 = 0;
				}
				if(x + w >= pwdith-1){
					c3 = c2 = 0;
				}
				if(y + h >= pheight-1){
					c4 = c3 = 0;
					
				}
			}
			
			//Do the operation over again for each of the four new grids.                 
			divideGrid(x, y, newWidth, newHeight, c1, edge1, middle, edge4, heights);
			divideGrid(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle, heights);
			divideGrid(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3, heights);
			divideGrid(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4, heights);
		}
	}
}
