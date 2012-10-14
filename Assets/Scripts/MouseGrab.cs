using UnityEngine;
using System.Collections;

public class MouseGrab : MonoBehaviour {
	public float springConstant = 100.0f;
	public float dampingConstant = 10.0f;
	public float rayLength = 10.0f;
	public float maxForce = 10.0f;
	
	public GameObject gimble;
	public Camera mainCamera;
	
	private GameObject selectedGameobject;
	private float selectedObjectDistance;
	private Vector3 selectedHitPos;
	private Vector3 gimbleHitPos;
	private Vector3 direction = Vector3.zero;
	Ray pointerRay;

	private bool buttonPrev = false;
	
	void Start() {
		
	}
	
	void Update() {
		pointerRay = mainCamera.ScreenPointToRay(Input.mousePosition);
		
		bool buttonCurrent = Input.GetMouseButton(0);
		if (buttonCurrent && !buttonPrev) {
			if (!selectedGameobject) {
				UpdateSelected();
			} else {
				UpdateDirection();
				if (direction == Vector3.zero) {
					UpdateSelected();
				}
			}

			
			if (selectedGameobject) {
				ShowGimble(selectedHitPos);
			} else {
				HideGimble();
			}
		} else if (buttonCurrent) {
			UpdateForce();

		} else if (!buttonCurrent && buttonPrev) {
		
		}
		buttonPrev = buttonCurrent;
	}
	
	void UpdateForce() {
		/*
		if (selectedGameobject) {
			selectedGameobject.GetComponent<BlockSelection>().AddSelectedHighlight();
		
			Vector3 diff = pointerRay.GetPoint(selectedObjectDistance)+selectedHitPos-selectedGameobject.transform.position;
			diff = new Vector3(diff.x*direction.x, diff.y*direction.y, diff.z*direction.z);
			Vector3 force = diff * springConstant - dampingConstant*selectedGameobject.rigidbody.velocity;
			if (force.magnitude > maxForce) {
				force = maxForce*force.normalized;	
			}
			//selected.rigidbody.AddForce(force);
			//gimble.transform.position = selectedGameobject.transform.position-selectedObjectHitPos;
		}	*/
	}
		
	void UpdateSelected() {
		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
		selectedGameobject = null;
		selectedObjectDistance = -1.0f;
		
		// Find the collided object that's closest to the origin of the ray
		for (int i=0; i < allCollidedRaycasts.Length; i++) {
			if (!allCollidedRaycasts[i].collider || allCollidedRaycasts[i].collider.gameObject.layer == 8)
				continue;
			//Vector3 objectHitPos = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - pointerRay.origin;
			Vector3 objectHitPos = allCollidedRaycasts[i].point - pointerRay.origin;
			if (objectHitPos.magnitude < selectedObjectDistance || selectedObjectDistance == -1.0) {
				selectedGameobject = allCollidedRaycasts[i].collider.gameObject;
				selectedObjectDistance = objectHitPos.magnitude;
				selectedHitPos = allCollidedRaycasts[i].point;
			}
		}
	}
	
	void ClearSelected() {
		selectedGameobject = null;
	}
	void UpdateDirection() {
		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
		
		GameObject gameobj = null;
		float shortestDistance = -1.0f;
		
		// Find the collided object that's closest to the origin of the ray
		for (int i=0; i < allCollidedRaycasts.Length; i++) {
			if (!allCollidedRaycasts[i].collider
				|| null == allCollidedRaycasts[i].collider.gameObject.GetComponent<GimbleSelection>()) //on top layer
				continue;
			//Vector3 objectHitPos = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - pointerRay.origin;
			Vector3 objectHitPos = allCollidedRaycasts[i].point - pointerRay.origin;
			if (objectHitPos.magnitude < shortestDistance || shortestDistance == -1.0) {
				gameobj = allCollidedRaycasts[i].collider.gameObject;
				shortestDistance = objectHitPos.magnitude;
				gimbleHitPos = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - allCollidedRaycasts[i].point;
			}
		}
		if (gameobj) {
			direction = gameobj.GetComponent<GimbleSelection>().direction;
		} else {
			direction = Vector3.zero;	
		}
	}
	
	void ShowGimble(Vector3 point) {
		gimble.SetActiveRecursively(true);	
		gimble.transform.position = point;
	}
	
	void HideGimble() {
		gimble.SetActiveRecursively(false);	
	}
}