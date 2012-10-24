using UnityEngine;
using System.Collections;

public class BlockPositions : MonoBehaviour {
	public float nearOriginalPosRange = 5.0f;
	public float tollerance = 0.01f;

	private Vector3 originalPos;
	
	// Use this for initialization
	void Start () {
		originalPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public bool AtOriginalPos() {
		Vector3 diff = transform.position - originalPos;
		return diff.magnitude <= tollerance;
	}
	
	public bool IsNearOriginalPos() {
		Vector3 diff = transform.position - originalPos;
		return diff.magnitude <= nearOriginalPosRange;
	}	
}
