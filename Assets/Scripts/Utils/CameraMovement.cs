using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple Camera movement that can be used to explore the maps while running the simulations.
/// </summary>
public class CameraMovement : MonoBehaviour {

	public float speed = 10.0f;


	public void FixedUpdate() {


		if(Input.GetKey(KeyCode.RightArrow)) {
			transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
		}
		if(Input.GetKey(KeyCode.LeftArrow)) {
			transform.position = new Vector3(transform.position.x - speed, transform.position.y, transform.position.z);
		}
		if(Input.GetKey(KeyCode.DownArrow)) {
			transform.position = new Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
		}
		if(Input.GetKey(KeyCode.UpArrow)) {
			transform.position = new Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
		}
	}
}
