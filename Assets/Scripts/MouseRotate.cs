using UnityEngine;
using System.Collections;

public class MouseRotate : MonoBehaviour {
	private Vector2 startMousePos;
	public float rotationScale = 10;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		transform.RotateAround(Vector3.zero, -transform.TransformDirection(Vector3.up), -Input.GetAxis("Mouse X")*rotationScale);
		transform.RotateAround(Vector3.zero, -transform.TransformDirection(Vector3.left), -Input.GetAxis("Mouse Y")*rotationScale);

	}
}
