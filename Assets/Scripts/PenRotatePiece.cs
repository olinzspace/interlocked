using UnityEngine;
using System.Collections;

public class PenRotatePiece : MonoBehaviour {
	public float springConstant = 1.0f;
	public float dampingConstant = 5f;
	public float rayLength = 10.0f;
	public float maxForce = 10.0f;
	public float snapAngle = 45.0f;
	public GameObject zspace;
	
	private bool buttonPrev = false;
	private GameObject selected;
	private GameObject collided;
	private Quaternion initQuat;
	private Quaternion initSelectedQuat;
	void Start () {

	}
	
	void Update () {
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
		selected.rigidbody.angularDrag = 30;
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

			Vector3 diff = this.transform.rotation.eulerAngles - initQuat.eulerAngles;
			float dx = Mathf.Round(diff.x/snapAngle)*snapAngle;
			float dy = Mathf.Round(diff.y/snapAngle)*snapAngle;
			float dz = Mathf.Round(diff.z/snapAngle)*snapAngle;
			Vector3 targetAngle = EulerAnglesInDomain(new Vector3(dx, dy, dz));
			
			Vector3 blockAngle = EulerAnglesInDomain (selected.gameObject.transform.rigidbody.rotation.eulerAngles - initSelectedQuat.eulerAngles);
			Vector3 angleDiff = EulerAnglesInDomain (targetAngle - blockAngle);
			
			Vector3 angVel = EulerAnglesInDomain (selected.gameObject.transform.rigidbody.angularVelocity);
			selected.rigidbody.AddTorque (springConstant*angleDiff - dampingConstant*angVel);
		}
	}
	
	private Vector3 EulerAnglesInDomain(Vector3 eulerAngles) {
		eulerAngles.x = (float)((int)(eulerAngles.x + 900) % 360) - 180.0f;
		eulerAngles.y = (float)((int)(eulerAngles.y + 900) % 360) - 180.0f;
		eulerAngles.z = (float)((int)(eulerAngles.z + 900) % 360) - 180.0f;
		return eulerAngles;
	}
}
