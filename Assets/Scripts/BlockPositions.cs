using UnityEngine;
using System.Collections;

public class BlockPositions : MonoBehaviour {
	public float maxForce = 125.0f;
	public float nearOriginalPosRange = 5.0f;

	
	public Vector3 originalPos, lastPos;
	public bool snapbackForce;
	
	// Use this for initialization
	void Start () {
		originalPos = transform.position;
		lastPos = originalPos;
		snapbackForce = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (AtOriginalPos () || !IsNearOriginalPos ()) {
			snapbackForce = false;
			gameObject.GetComponent<BlockSelection>().RemoveHighlight();
		}
		if (snapbackForce) {
			Vector3 diff = transform.position - originalPos;
			Vector3 force = - maxForce * diff.normalized;
			gameObject.rigidbody.AddForce (force);
		}
	}
	
	public bool AtOriginalPos() {
		Vector3 diff = transform.position - originalPos;
		return diff.magnitude <= 1.0e-2; // Tollerance
	}
	
	public bool IsNearOriginalPos() {
		Vector3 diff = transform.position - originalPos;
		return diff.magnitude <= nearOriginalPosRange;
	}	
	
	public bool WasNearOriginalPos() {
		Vector3 diff = lastPos - originalPos;
		return diff.magnitude <= nearOriginalPosRange;
	}
}
