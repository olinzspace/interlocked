using UnityEngine;
using System.Collections;

public class PenGrab : MonoBehaviour {
	public float springConstant = 100.0f;
	public float dampingConstant = 10.0f;
	public float rayLength = 10.0f;
	public float maxForce = 10.0f;
	public GameObject zspace;
	
	private bool buttonPrev = false;
	private GameObject selected;
	private GameObject collided;
	private float selectedObjectDistance;
	private Vector3 selectedObjectHitPos;
	
	void Start () {

	}
	
	void Update () {
		ClearBlockHighlighting();
		Ray pointerRay = new Ray(transform.position, transform.rotation*Vector3.forward);
		

		RaycastHit collidedRaycast = GetCollidedRaycast(pointerRay);
		DrawRay(pointerRay, collidedRaycast);
		if (collidedRaycast.collider && !selected) {
			collidedRaycast.collider.gameObject.SendMessageUpwards ("AddHoverHighlight");
		}
		
		bool buttonCurrent = zspace.GetComponent<ZSCore>().IsTrackerTargetButtonPressed(ZSCore.TrackerTargetType.Primary, 0);
		
		if (buttonCurrent && !buttonPrev) {
			ButtonJustPressed(collidedRaycast);
		} else if (buttonCurrent) {
			ButtonPressed(pointerRay);
		} else if (!buttonCurrent && buttonPrev) {
			ButtonJustReleased(collidedRaycast);
		}
		buttonPrev = buttonCurrent;
	}
	
	void DrawRay(Ray ray, RaycastHit raycast) {
		LineRenderer line = GetComponent<LineRenderer>();
		line.SetPosition (0, ray.origin);
		if(raycast.collider) {
			float newLength = (ray.origin-raycast.point).magnitude;
			line.SetPosition (1, ray.GetPoint(newLength));
		} else {
			line.SetPosition (1, ray.GetPoint(rayLength));
		}
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
		selected = raycastHit.collider.gameObject.transform.root.gameObject;
		selectedObjectDistance =  raycastHit.distance;
		selectedObjectHitPos = raycastHit.collider.gameObject.transform.root.transform.position - raycastHit.point;
		selected.rigidbody.isKinematic = false;	
	}
		
	void ButtonJustReleased(RaycastHit raycastHit) {
		if (selected) {
			selected.rigidbody.isKinematic = true;
			selected = null;
		}
	}
	void ButtonPressed(Ray pointerRay) {
		if (selected) {
			selected.GetComponent<BlockSelection>().AddSelectedHighlight();
			Vector3 diff = pointerRay.GetPoint(selectedObjectDistance)+selectedObjectHitPos - selected.transform.position;
			Vector3 force = diff * springConstant - dampingConstant*selected.rigidbody.velocity;
			if (force.magnitude > maxForce) {
				force = maxForce*force.normalized;	
			}
			selected.rigidbody.AddForce(force);
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

