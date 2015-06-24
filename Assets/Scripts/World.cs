using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class World : MonoBehaviour {
	public const int lg_xdim = 6;
	public const int lg_ydim = 6;
	public const int lg_zdim = 6;
	public const int xdim = 3<<lg_xdim;
	public const int ydim = 3<<lg_ydim;
	public const int zdim = 3<<lg_zdim;
	public const int gxdim = 1<<(lg_xdim+Chunk.lg_xdim);
	public const int gydim = 1<<(lg_ydim+Chunk.lg_ydim);
	public const int gzdim = 1<<(lg_zdim+Chunk.lg_zdim);
	Chunk[] chunks = new Chunk[xdim*ydim*zdim];
	public Material world_material;

	List <GameObject> chunkList;



	public Transform player;

	public float viewDistance = 100.0f;


	float updateTime;




	


	// rx + l nx = cx -> l = (cx - rx) / nx

	/*public bool RayCast(Vector3 origin, Vector3 direction, float max_distance, out int oi, out int oj, out int ok) {
		float rx = origin.x;
		float ry = origin.y;
		float rz = origin.z;
		int di = (direction.x >= 0 ? 1 : -1);
		int dj = (direction.y >= 0 ? 1 : -1);
		int dk = (direction.z >= 0 ? 1 : -1);
		float denomx = Mathf.Abs(direction.x);
		float denomy = Mathf.Abs(direction.y);
		float denomz = Mathf.Abs(direction.z);
        int i = Mathf.FloorToInt (rx);
		int j = Mathf.FloorToInt (ry);
		int k = Mathf.FloorToInt (rz);
		int ci = i >> Chunk.lg_xdim;
		int cj = j >> Chunk.lg_ydim;
		int ck = k >> Chunk.lg_zdim;
		int qi = i & (Chunk.xdim-1);
		int qj = j & (Chunk.ydim-1);
		int qk = k & (Chunk.zdim-1);

		Chunk ch = get_chunk (i, j, k);
		if (ch.GetValue (qi, qj, qk)) {
			oi = i;
			oj = j;
			ok = k;
			return true;
        }

		float numerx = i + di - rx;
		float numery = j + dj - ry;
        float numerz = k + dk - rz;

		if (numerx * denomy < numery * denomx && numerx * denomz < numerz * denomx) {
			rx = i += di;
			ry += direction.y * (numerx / denomx);
			rz += direction.z * (numerx / denomx);
		} else if (numery * denomz < numerz * denomy) {
			ry = j += dj;
			rx += direction.x * (numery / denomy);
			rz += direction.z * (numery / denomy);
		} else {
			rz = k += dk;
			rx += direction.x * (numerz / denomz);
			ry += direction.y * (numerz / denomz);
        }
    }*/
    
    public Chunk get_chunk(int ci, int cj, int ck) {
		int idx = (ck * ydim + cj) * xdim + ci;
		return chunks [idx];
	}

	public Chunk writable_chunk(int ci, int cj, int ck) {
		int idx = (ck * ydim + cj) * xdim + ci;
		//Debug.Log("make_chunk " + ci + ", " + cj + ", " + ck);
		Chunk ch = chunks [idx];
		if (ch == null) {
			GameObject obj = new GameObject("ch_" + ci + "_" + cj + "_" + ck);

			ch = obj.AddComponent<Chunk>();
			obj.AddComponent<MeshFilter>();
			MeshRenderer mr = obj.AddComponent<MeshRenderer>();
			mr.material = world_material;
			ch.transform.position = new Vector3(
				(ci) * Chunk.xdim,
				(cj) * Chunk.ydim,
				(ck) * Chunk.zdim
				) + this.transform.position;
			ch.world = this;
			chunks[idx] = ch;
		
			obj.transform.position = new Vector3((ci) * Chunk.xdim, (cj) * Chunk.ydim, (ck) * Chunk.zdim) + this.transform.position;
			obj.transform.parent = this.transform;

			chunkList.Add(obj);


		}
		ch.init = true;
		return ch;
	}

	public void set_voxel(int i, int j, int k, int value) {
		//Debug.Log("set_voxel " + i + ", " + j + ", " + k);
		int ci = i >> Chunk.lg_xdim;
		int cj = j >> Chunk.lg_ydim;
		int ck = k >> Chunk.lg_zdim;
		int qi = i & (Chunk.xdim-1);
		int qj = j & (Chunk.ydim-1);
		int qk = k & (Chunk.zdim-1);
		Chunk ch = writable_chunk (ci, cj, ck);
		int qidx = (qk * Chunk.ydim + qj) * Chunk.xdim + qi;
		ch.values [qidx] = (byte)value;
	}
	

	/*public void remove_voxel(int i, int j, int k) {
		//Debug.Log("set_voxel " + i + ", " + j + ", " + k);
		int ci = i >> Chunk.lg_xdim;
		int cj = j >> Chunk.lg_ydim;
		int ck = k >> Chunk.lg_zdim;
		int qi = i & (Chunk.xdim-1);
		int qj = j & (Chunk.ydim-1);
		int qk = k & (Chunk.zdim-1);
		Chunk ch = writable_chunk (ci, cj, ck);

		int qidx = (qk * Chunk.ydim + qj) * Chunk.xdim + qi;
		ch.values [qidx] = 0;
		bool empty = false;
	}*/

	// Use this for initialization
	void Start () {
		updateTime = Time.time;
		chunkList = new List<GameObject> ();
		/*for (int i = -20; i <= 20; ++i) {
			for (int k = -20; k <= 20; ++k) {
				set_voxel(i + gxdim/2, 4, k + gzdim/2, 1);
				set_voxel(i + gxdim/2, 3, k + gzdim/2, 2);
				set_voxel(i + gxdim/2, 2, k + gzdim/2, 2);
				set_voxel(i + gxdim/2, 1, k + gzdim/2, 3);
				set_voxel(i + gxdim/2, 0, k + gzdim/2, 3);
			}
		}
		set_voxel(1 + gxdim/2, 5, 0 + gzdim/2, 3);
		set_voxel(1 + gxdim/2, 6, 0 + gzdim/2, 3);*/


        /*set_voxel(1 + gxdim/2, 8, 0 + gzdim/2, 4);
		set_voxel(0 + gxdim/2, 7, 0 + gzdim/2, 5);
		set_voxel(1 + gxdim/2, 7, 0 + gzdim/2, 5);
		set_voxel(2 + gxdim/2, 7, 0 + gzdim/2, 5);
		set_voxel(1 + gxdim/2, 6, 0 + gzdim/2, 5);*/

		set_voxel (0, 0, 0, 2);
		print (gxdim);



	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - updateTime > 0.5f) {
			updateList();
			updateTime = Time.time;
		}

		//set_voxel((int)player.position.x, (int)player.position.y, (int)player.position.z, 1);
	}

	void updateList(){

		//Thread sortThread = new Thread(new ThreadStart(SortList));
		//sortThread.Start();


		/*for(int i = 0; i < chunkList.Count; i++)
		{
			chunkList[i].GetComponent<MeshRenderer>().enabled= false;
		}*/
	
		float viewDist = viewDistance * viewDistance;
		for(int i = 0; i < chunkList.Count; i++)
		{

			if(CalcDistance(chunkList[i].transform.position, player.position) < viewDist){
				chunkList[i].GetComponent<MeshRenderer>().enabled= true;
			}
			else{
				chunkList[i].GetComponent<MeshRenderer>().enabled= false;
			}
		}


	}

	public int getgxdim(){

		return gxdim;
	}

	public void SortList() {
		print ("trying");
		chunkList.Sort(delegate(GameObject c1, GameObject c2){
			return CalcDistance(player.transform.position, c1.transform.position).CompareTo
				((CalcDistance(player.transform.position, c2.transform.position)));   
		});
	}

	float CalcDistance(Vector3 a, Vector3 b){
		float xD = a.x - b.x;
		float yD = a.y - b.y;
		float zD = a.z - b.z;
		return (xD*xD + yD*yD + zD*zD);
	}



		

	
}
