using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {

	Transform player;

	Vector3 currentPosition;
	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Player").transform;
		currentPosition = player.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (currentPosition != player.position) {
			currentPosition = player.position;
			//this.GetComponent<TerrainScript>().generateTerrainDinamically(currentPosition);
		}
	}
}
