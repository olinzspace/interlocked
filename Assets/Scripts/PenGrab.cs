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
	private RaycastHit collidedRaycast;
	private float selectedObjectDistance;
	private Vector3 selectedObjectHitPos;
	
	void Start () {

	}
	
	void Update () {
		Ray pointerRay = new Ray(transform.position, transform.rotation*Vector3.forward);
		
		LineRenderer line = GetComponent<LineRenderer>();
		line.SetPosition (0, pointerRay.origin);
		line.SetPosition (1, pointerRay.GetPoint(rayLength));
		
		RaycastHit[] allCollidedRaycasts;
		allCollidedRaycasts = Physics.RaycastAll (pointerRay);
		
		
		if (allCollidedRaycasts.Length > 0) {
			collidedRaycast = allCollidedRaycasts[0];
			CollidedObjEnterCallback(collidedRaycast.collider);
			//Debug.Log("on obj");
		} else {
			//collidedRaycast = null;
			CollidedObjExitCallback();
		}
			
		
		bool buttonCurrent = zspace.GetComponent<ZSCore>().IsTargetButtonPressed(ZSCore.TrackerTargetType.Primary, 0);
		bool justSelected = buttonCurrent && !buttonPrev;
		if (justSelected) {
			
			selected = collidedRaycast.collider.gameObject;
			if(selected) {
				selectedObjectDistance =  collidedRaycast.distance;
				selectedObjectHitPos = collidedRaycast.collider.gameObject.transform.position - collidedRaycast.point;
				selected.rigidbody.isKinematic = false;
			}
		}
		if(buttonCurrent) {
			if (selected ) {
				
				Vector3 diff = pointerRay.GetPoint(selectedObjectDistance)+selectedObjectHitPos - selected.transform.position;
				Vector3 force = diff*springConstant - dampingConstant*selected.rigidbody.velocity;
				selected.rigidbody.AddForce(force);
			}
		}
		bool justDeselected = !buttonCurrent && buttonPrev;
		if (justDeselected) {
			if (selected) {
				selected.rigidbody.isKinematic = true;
				selected = null;
			}
		}
		buttonPrev = buttonCurrent;
		
		
	}
	
	void CollidedObjEnterCallback(Collider other) {
		
		if (!other.GetComponent(typeof(Rigidbody))) {
			collided = null;
		} else if (!collided) {
			collided = other.gameObject;
		}
		
	}
	
	void CollidedObjExitCallback() {
		collided = null;
	}
}
