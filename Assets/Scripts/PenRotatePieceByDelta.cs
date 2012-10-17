using UnityEngine;
using System.Collections;

public class PenRotatePieceByDelta : MonoBehaviour {
	public float springConstant = 100.0f;
	public float dampingConstant = 10.0f;
	public float rayLength = 10.0f;
	public float maxForce = 10.0f;
	public GameObject zspace;
	
	private bool buttonPrev = false;
	private GameObject selected;
	private GameObject collided;
	private Vector3 selectedPoint;
	
	void Start () {

	}
	
	void Update () {
		ClearBlockHighlighting();
		Ray pointerRay = new Ray(transform.position, transform.rotation*Vector3.forward);
		

		RaycastHit collidedRaycast = GetCollidedRaycast(pointerRay);
		
		bool buttonCurrent = zspace.GetComponent<ZSCore>().IsTrackerTargetButtonPressed(ZSCore.TrackerTargetType.Primary, 1);
		
		if (buttonCurrent && !buttonPrev) {
			ButtonJustPressed(collidedRaycast);
		} else if (buttonCurrent) {
			ButtonPressed(pointerRay);
		} else if (!buttonCurrent && buttonPrev) {
			ButtonJustReleased(collidedRaycast);
		}
		buttonPrev = buttonCurrent;
	}
	
	RaycastHit GetCollidedRaycast(Ray pointerRay) {
		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
		
		RaycastHit collidedRaycast = new RaycastHit();
		float shortestDistance = -1.0f;
		
		// Find the collided object that's closest to the origin of the ray
		for (int i=0; i < allCollidedRaycasts.Length; i++) {
			if (!allCollidedRaycasts[i].collider)
				continue;
			//Vector3 objectHitPos = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - pointerRay.origin;
			Vector3 objectHitPos = allCollidedRaycasts[i].point - pointerRay.origin;
			if (objectHitPos.magnitude < shortestDistance || shortestDistance == -1.0) {
				collidedRaycast = allCollidedRaycasts[i];
				shortestDistance = objectHitPos.magnitude;
			}
		}
		return collidedRaycast;
	}
	
	void ButtonJustPressed(RaycastHit raycastHit) {
		if (!raycastHit.collider) {
			return;
		}
		selectedPoint = transform.position;
		selected = raycastHit.collider.gameObject.transform.root.gameObject;
		//selectedObjectDistance =  raycastHit.distance;
		//selectedObjectHitPos = raycastHit.collider.gameObject.transform.root.transform.position - raycastHit.point;
		selected.rigidbody.isKinematic = false;	
		selected.rigidbody.angularDrag = 10;
	}
		
	void ButtonJustReleased(RaycastHit raycastHit) {
		if (selected) {
			selected.rigidbody.isKinematic = true;
			selected.rigidbody.angularDrag = 100;
			selected = null;

		}
	}
	void ButtonPressed(Ray pointerRay) {
		if (selected) {
			selected.GetComponent<BlockSelection>().AddSelectedHighlight();
			Vector3 diff = selectedPoint - transform.position;
			
			Vector3 rotation = diff * springConstant;// - dampingConstant*selected.rigidbody.angularVelocity;
		//	if (force.magnitude > maxForce) {
		//		force = maxForce*force.normalized;	
		//	}
			
			Vector3 force = new Vector3(0, rotation.z, -rotation.y);
			selected.rigidbody.AddTorque(force);
		}
	}
	
	void ClearBlockHighlighting() {
		Object[] objects = FindObjectsOfType(typeof(BlockSelection));
		foreach (Object block in objects) {
			BlockSelection blockSelection = block as BlockSelection;
			blockSelection.RemoveHighlight();
		}	
	}
}
