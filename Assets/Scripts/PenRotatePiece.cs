using UnityEngine;
using System.Collections;

public class PenRotatePiece : MonoBehaviour {
	public float springConstant = 100.0f;
	public float dampingConstant = 10.0f;
	public float rayLength = 10.0f;
	public float maxForce = 10.0f;
	public GameObject zspace;
	
	private bool buttonPrev = false;
	private GameObject selected;
	private GameObject collided;
	private Quaternion initQuat;
	private Quaternion initSelectedQuat;
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
		initQuat = transform.rotation;

		selected = raycastHit.collider.gameObject.transform.root.gameObject;
		initSelectedQuat = selected.transform.rotation;
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
			//Quaternion difference = Quaternion.Inverse(transform.rotation)*initQuat;
			Vector3 diff = transform.rotation.eulerAngles - initQuat.eulerAngles;
			Vector3 selectedDiff = selected.transform.rotation.eulerAngles - initSelectedQuat.eulerAngles;
			
			Vector3 delta = selectedDiff - diff;
			
			//Vector3 diff = pointerRay.GetPoint(selectedObjectDistance)+selectedObjectHitPos - selected.transform.rotation;
			//Vector3 force = diff * springConstant - dampingConstant*selected.rigidbody.velocity;
			//if (force.magnitude > maxForce) {
			//	force = maxForce*force.normalized;	
			//}
			
			//Vector3 force = new Vector3(0, rotation.z, -rotation.y);
			//Vector3 diff = difference.eulerAngles;
			float dy = delta.y;
			if (delta.y > 180) {
				dy -= 360; 	
			} else if (delta.y < -180) {
				dy += 360;	
			}
			
			float dx = delta.y;
			if (delta.x > 180) {
				dx -= 360; 	
			} else if (delta.y < -180) {
				dx += 360;	
			}
			
			//rot = 180 * ((Input.mousePosition.x - Screen.width / 2) / (Screen.width / 2));
			
			
			Quaternion q = Quaternion.identity;
			q.eulerAngles =initSelectedQuat.eulerAngles + diff;
			selected.rigidbody.MoveRotation(q);
			//selected.rigidbody.AddTorque(-dx, -dy, 0);

			Debug.Log(delta);
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
