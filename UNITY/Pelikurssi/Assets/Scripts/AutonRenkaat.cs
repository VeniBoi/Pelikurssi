using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonRenkaat : MonoBehaviour {

	public int speed = 50;
	
	// Update is called once per frame
	void Update () {

		transform.Rotate(Vector3.back, speed * Time.deltaTime);
		
	}
}
