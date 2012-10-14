using UnityEngine;
using System.Collections;

public class GimbleSelection : MonoBehaviour {
	private Vector3 scale;
	public Vector3 gimbleDirection;
	// Use this for initialization
	void Start () {
		scale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void Hover () {
		transform.localScale = 2*scale;
	}
}
