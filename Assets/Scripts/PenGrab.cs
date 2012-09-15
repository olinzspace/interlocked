using UnityEngine;
using System.Collections;

public class PenGrab : MonoBehaviour {
	public float springConstant = 100.0f;
	public float dampingConstant = 10.0f;
	public float rayLength = 10.0f;
	
	public GameObject zspace;
	
	private bool buttonPrev = false;
	private GameObject selected;
	private GameObject collided;
	private float selectedObjectDistance;
	private Vector3 selectedObjectHitPos;
	
	void Start () {

	}
	
	void Update () {
		Ray pointerRay = new Ray(transform.position, transform.rotation*Vector3.forward);
		DrawRay(pointerRay);

		RaycastHit collidedRaycast = GetCollidedRaycast(pointerRay);
		
		bool buttonCurrent = zspace.GetComponent<ZSCore>().IsTargetButtonPressed(ZSCore.TrackerTargetType.Primary, 0);
		
		if (buttonCurrent && !buttonPrev) {
			ButtonJustPressed(collidedRaycast);
		} else if (buttonCurrent) {
			ButtonPressed(pointerRay);
		} else if (!buttonCurrent && buttonPrev) {
			ButtonJustReleased(collidedRaycast);
		}
		buttonPrev = buttonCurrent;
	}
	
	void DrawRay(Ray ray) {
		LineRenderer line = GetComponent<LineRenderer>();
		line.SetPosition (0, ray.origin);
		line.SetPosition (1, ray.GetPoint(rayLength));
	}
	
	RaycastHit GetCollidedRaycast(Ray pointerRay) {
		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
		if (allCollidedRaycasts.Length > 0) {
			return allCollidedRaycasts[0];
		} else {
			return new RaycastHit();
		}
	}
	
	void ButtonJustPressed(RaycastHit raycastHit) {
		if (!raycastHit.collider) {
			return;
		}
		
		selected = raycastHit.collider.gameObject;
		selectedObjectDistance =  raycastHit.distance;
		selectedObjectHitPos = raycastHit.collider.gameObject.transform.position - raycastHit.point;
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
			Vector3 diff = pointerRay.GetPoint(selectedObjectDistance)+selectedObjectHitPos - selected.transform.position;
			Vector3 force = diff * springConstant - dampingConstant*selected.rigidbody.velocity;
			selected.rigidbody.AddForce(force);
		}
	}
}

