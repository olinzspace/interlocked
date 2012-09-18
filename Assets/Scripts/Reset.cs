using UnityEngine;
using System.Collections;

public class Reset : MonoBehaviour {
	private Vector3 originalPos;
	private Quaternion originalRot;
	// Use this for initialization
	void Start () {
		originalPos = transform.position;
		originalRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.R)) {
			transform.position = originalPos;	
			transform.rotation = originalRot;
		}
	}
}
