using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ZSCore))]
public class StylusTransform : MonoBehaviour {
	public Transform stylusObject;
	public Transform mainCamera;
	
	// Update is called once per frame
	void Update () {
		Matrix4x4 pose = GetComponent<ZSCore>().GetTrackerTargetWorldPose(ZSCore.TrackerTargetType.Primary);
		Vector3 fwd = pose * Vector3.forward;
		Vector3 up = pose * Vector3.up;
		stylusObject.position = pose.MultiplyPoint(Vector3.zero);
		stylusObject.rotation = Quaternion.LookRotation(fwd, up);
	}
}
