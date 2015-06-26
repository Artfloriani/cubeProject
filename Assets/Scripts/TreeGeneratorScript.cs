using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeGeneratorScript : MonoBehaviour {


	World world;
	Dictionary<Vector3, int> treePositions;
	// Use this for initialization
	void Start () {


		world = this.GetComponent<World> ();

		/*
		for (int i = 100; i <= 500; ++i) {
			for (int k = 100; k <= 500; ++k) {
				if(Random.Range(0, 2000) == 20){
					generateTree(new Vector3(i,0,k));
				}
				world.set_voxel(i , 0, k, 3);

			}
		}*/




	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void generateTree(Vector3 pos){
		world = this.GetComponent<World> ();
		int height = Random.Range (5, 15);
		float modifier = Random.Range (1.0f, 1.5f);
		float seed = Random.Range(300, 400);

		for (int i = 0; i < height; i++) {
			for(int j = 0; j < 360; j+= 6){
				int trunkSize = (int)(height/20.0f) + (int) (Mathf.Sqrt(Mathf.Sqrt(height-i)));

				trunkSize += (int)(Mathf.PerlinNoise((i/(float)height),j/360.0f) * (2));
				world.set_voxel(  (int)(pos.x+trunkSize*Mathf.Cos ((j) * (Mathf.PI / 180.0f))), (int) (pos.y+ i), (int)(pos.z+trunkSize*Mathf.Sin ((j) * (Mathf.PI / 180.0f))), 2);


			}
		}
		float mod1 = Random.Range(0.7f, 0.85f);
		float mod2 = Random.Range(-0.8f, 0.8f);
		float mod3 = 1;
		float mod4 = Random.Range(-0.8f, 0.8f);
		float mod5 = Random.Range(-0.8f, 0.8f);
		for (float i = 0; i < 360; i += 10.0f) {
			for (float j = 0; j < 180; j += 10.0f) {
				float radius = (height/4.0f) + (height/10.0f) * Mathf.PerlinNoise ((float)seed * (i / 360.0f), (float)seed * (j / 180.0f));
				//Use the equation of the Spherical Coordinate system to display a sphere
				Vector3 posVec = new Vector3 ((float)radius * Mathf.Sin ((i) * Mathf.PI / 180.0f) * Mathf.Cos ((j) * Mathf.PI / 180.0f), (radius/2) * Mathf.Sin ((i) * Mathf.PI / 180.0f) * Mathf.Sin ((j) * Mathf.PI / 180.0f), radius * Mathf.Cos ((i) * Mathf.PI / 180.0f));

				float mod6 = Random.Range(-1.0f, 1.0f);


				world.set_voxel(  (int)(pos.x+posVec.x + radius*mod6), (int) (height + pos.y + posVec.y + radius*mod5) , (int)(pos.z + posVec.z + radius*mod6), 6);

				world.set_voxel(  (int)(pos.x+posVec.x + radius*mod2), (int) ((height + pos.y + posVec.y+ radius*mod1)), (int)(pos.z + posVec.z+ radius*mod2), 6);

				world.set_voxel(  (int)(pos.x+posVec.x - radius*mod2), (int) (height + pos.y + posVec.y), (int)(pos.z + posVec.z), 6);
				world.set_voxel(  (int)(pos.x+posVec.x + radius*mod3), (int) (height + pos.y + posVec.y), (int)(pos.z + posVec.z), 6);
				world.set_voxel(  (int)(pos.x+posVec.x + radius*mod4), (int) (height + pos.y + posVec.y), (int)(pos.z + posVec.z - radius* mod5), 6);

				//world.set_voxel(  (int)(pos.x+posVec.x + radius*mod6), (int) (height + pos.y + posVec.y+ radius*mod6), (int)(pos.z + posVec.z), 1);
			}
		}
		//world.set_voxel((int)pos.x, (int)pos.y, (int)pos.z, 3);
	}

}
