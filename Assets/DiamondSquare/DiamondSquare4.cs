// Original header: "Adapted for Processing from "Plasma Fractal" by Giles Whitaker - Written January, 2002 by Justin Seyster (thanks for releasing into public domain, Justin)."
// Unity C# version - 4.4.2012 - mgear - http://unitycoder.com/blog

using UnityEngine;
using System.Collections;

public class DiamondSquare4 : MonoBehaviour {
	
	Color32[] cols;
	int pwidth=128;
	int pheight=128;
	float pwidthpheight;
	int GRAIN=8;
	Texture2D texture;
	int dummy=0;
	
	// Use this for initialization
	void Start () 
	{
		
		 pwidthpheight= (float)pwidth+pheight;
		
        texture = new Texture2D(pwidth, pheight);
        GetComponent<Renderer>().material.mainTexture = texture;
//		renderer.material.mainTexture.filterMode = FilterMode.Point;
		
		cols = new Color32[pwidth*pheight];

		// draw one here already..
		drawPlasma(pwidth, pheight);
		texture.SetPixels32(cols);
		texture.Apply();
	}
	
	// Update is called once per frame
	void Update () 
	{
		// hold button down to generate new
		if (Input.GetMouseButton(0))
		{
			drawPlasma(pwidth, pheight);
			texture.SetPixels32(cols);
			texture.Apply();
		}
	}


	//	--- actual code starts ----
	float displace(float num)
	{
//			if (dummy<300) {print(num); dummy++;}
//			Debug.Break();
		float max = num / pwidthpheight * GRAIN;
		return Random.Range(-0.5f, 0.5f)* max;
	}

	//Returns a color based on a color value, c.
	Color computeColor(float c)
	{      
	  float Red = 0;
	  float Green = 0;
	  float Blue = 0;
			 
	  if (c < 0.5f)
	  {
		Red = c * 2;
	  }
	  else
	  {
		Red = (1.0f - c) * 2;
	  }
	  if (c >= 0.3 && c < 0.8f)
	  {
		Green = (c - 0.3f) * 2;
	  }
	  else if (c < 0.3f)
	  {
		 Green = (0.3f - c) * 2;
	  }
	  else
	  {
		 Green = (1.3f - c) * 2;
	  }
			 
	  if (c >= 0.5f)
	  {
		 Blue = (c - 0.5f) * 2;
	  }
	  else
	  {
		 Blue = (0.5f - c) * 2;
	  }
			 
	   return new Color(Red, Green, Blue);
	}

	//This is something of a "helper function" to create an initial grid
	//before the recursive function is called. 
	void drawPlasma(float w, float h)
	{
	   float c1, c2, c3, c4;
			 
	   //Assign the four corners of the intial grid random color values
	   //These will end up being the colors of the four corners of the applet.     
	   c1 = Random.value;
	   c2 = Random.value;
	   c3 = Random.value;
	   c4 = Random.value;
		
	   divideGrid(0.0f, 0.0f, w , h , c1, c2, c3, c4);
	}

    //This is the recursive function that implements the random midpoint
    //displacement algorithm.  It will call itself until the grid pieces
    //become smaller than one pixel.   
	void divideGrid(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
	{
	 
	   float newWidth = w * 0.5f;
	   float newHeight = h * 0.5f;
	 
	   if (w < 1.0f || h < 1.0f)
	   {
		 //The four corners of the grid piece will be averaged and drawn as a single pixel.
		 float c = (c1 + c2 + c3 + c4) * 0.25f;
			cols[(int)x+(int)y*pwidth] = computeColor(c);
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
	 
		 //Do the operation over again for each of the four new grids.                 
		 divideGrid(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
		 divideGrid(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
		 divideGrid(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
		 divideGrid(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
	   }
	}

	
}
