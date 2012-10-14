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
	private Vector3 moveDirection = Vector3.zero;
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
				if (moveDirection == Vector3.zero) {
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
			if (selectedGameobject) {
				selectedGameobject.rigidbody.isKinematic = true;
			}
		}
		buttonPrev = buttonCurrent;
	}
	
	void UpdateForce() {

		if (selectedGameobject && moveDirection != Vector3.zero) {
			selectedGameobject.rigidbody.isKinematic = false;	
			selectedGameobject.GetComponent<BlockSelection>().AddSelectedHighlight();
		
			Vector3 diff = pointerRay.GetPoint(selectedObjectDistance) + gimbleHitPos - selectedGameobject.transform.position;
			
			Vector3 force = diff * springConstant - dampingConstant*selectedGameobject.rigidbody.velocity;
			force = new Vector3(force.x*moveDirection.x, force.y*moveDirection.y, force.z*moveDirection.z);
			if (force.magnitude > maxForce) {
				force = maxForce*force.normalized;	
			}
			
			selectedGameobject.rigidbody.AddForce(force);
			selectedGameobject.rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
			selectedGameobject.rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
			selectedGameobject.rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
			
			if( moveDirection == new Vector3(1,0,0)) {
				selectedGameobject.rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionX;
			} else if( moveDirection == new Vector3(0, 1, 0) ) {
				selectedGameobject.rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;	
			} else if( moveDirection == new Vector3(0, 0, 1) ) {
				selectedGameobject.rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
			}
			
			gimble.transform.position = selectedGameobject.transform.position-gimbleHitPos;
		}	
	}
		
	void UpdateSelected() {
		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
		selectedGameobject = null;
		selectedObjectDistance = -1.0f;
		
		// Find the collided object that's closest to the origin of the ray
		for (int i=0; i < allCollidedRaycasts.Length; i++) {
			if (!allCollidedRaycasts[i].collider && 
				!allCollidedRaycasts[i].collider.gameObject.transform.root.gameObject.GetComponent<BlockSelection>())
				continue;
			if (allCollidedRaycasts[i].collider.gameObject.layer == 8)
				continue;
			Vector3 objectHitPos = allCollidedRaycasts[i].point - pointerRay.origin;
			if (objectHitPos.magnitude < selectedObjectDistance || selectedObjectDistance == -1.0) {
				selectedGameobject = allCollidedRaycasts[i].collider.gameObject.transform.root.gameObject;
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
				gimbleHitPos = selectedGameobject.transform.position - allCollidedRaycasts[i].point;
			}
		}
		if (gameobj) {
			moveDirection = gameobj.GetComponent<GimbleSelection>().gimbleDirection;

		} else {
			moveDirection = Vector3.zero;	
			gimbleHitPos = Vector3.zero;
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