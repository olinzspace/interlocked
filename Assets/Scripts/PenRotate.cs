using UnityEngine;
using System.Collections;

public class PenRotate : MonoBehaviour {
	public float rotationScale = 150;
	public GameObject mainCamera;
	
	private Vector3 startMousePos;
	private ZSCore zspace;
	
	// Use this for initialization
	void Start () {
		zspace = GameObject.Find("/ZSCore").GetComponent<ZSCore>();
	}
	
	// Update is called once per frame
	void Update () {
		bool currentButton = zspace.IsTrackerTargetButtonPressed(ZSCore.TrackerTargetType.Primary, 1);
		Vector3 localPos = zspace.GetTrackerTargetPose(ZSCore.TrackerTargetType.Primary).GetColumn(3);
		if (currentButton) {
			Vector3 diff = startMousePos - localPos;
			mainCamera.transform.RotateAround(Vector3.zero, -mainCamera.transform.TransformDirection(Vector3.up), diff.x*rotationScale);
			mainCamera.transform.RotateAround(Vector3.zero, -mainCamera.transform.TransformDirection(Vector3.left), diff.y*rotationScale);
		}
		startMousePos = localPos;
	}
}
