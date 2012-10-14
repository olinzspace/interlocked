//using UnityEngine;
//using System.Collections;
//
//public class MouseGrabTrasg : MonoBehaviour {
//	public float springConstant = 100.0f;
//	public float dampingConstant = 10.0f;
//	public float rayLength = 10.0f;
//	public float maxForce = 10.0f;
//	public Camera mainCamera;
//	public GameObject gimble;
//	
//	private bool buttonPrev = false;
//	private GameObject selected;
//	private GameObject collided;
//	private float selectedObjectDistance;
//	private Vector3 selectedObjectHitPos;
//	private Vector3 direction = new Vector3(0,0,0);
//	private Vector3 gimbleHit;
//	void Start () {
//
//	}
//	
//	void Update () {
//		ClearBlockHighlighting();
//		Ray pointerRay = mainCamera.ScreenPointToRay(Input.mousePosition);
//		
//
//		RaycastHit collidedRaycast = GetCollidedRaycast(pointerRay);
//		GameObject gimble = GetGimble(pointerRay);
//		
//		if (gimble) {
//			
//			GimbleSelection gimbleComp = gimble.GetComponent<GimbleSelection>();
//			if (gimbleComp) {
//				gimbleComp.Hover();
//				Debug.Log(gimble);
//			} else {
//				gimble = null;	
//			}
//		}
//		if (collidedRaycast.collider && !selected) {
//			collidedRaycast.collider.gameObject.SendMessageUpwards ("AddHoverHighlight");
//		}
//		
//		bool buttonCurrent = Input.GetMouseButton(0);
//		if (buttonCurrent && !buttonPrev) {
//			if(gimble) {
//				direction =  gimble.GetComponent<GimbleSelection>().direction;
//				selectedObjectHitPos = gimbleHit;
//			} else {
//				ButtonJustPressed(collidedRaycast);
//			}
//		} else if (buttonCurrent) {
//			if(selected) {
//				ButtonPressed(pointerRay);
//			}
//		} else if (!buttonCurrent && buttonPrev) {
//			if(direction != Vector3.zero) {
//				ButtonJustReleased(collidedRaycast);
//				direction = Vector3.zero;
//			}
//			//	direction =  gimble.GetComponent<GimbleSelection>().direction;
//			//}
//			//
//		}
//		buttonPrev = buttonCurrent;
//	}
//	
//	RaycastHit GetCollidedRaycast(Ray pointerRay) {
//		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
//		
//		RaycastHit collidedRaycast = new RaycastHit();
//		float shortestDistance = -1.0f;
//		
//		// Find the collided object that's closest to the origin of the ray
//		for (int i=0; i < allCollidedRaycasts.Length; i++) {
//			if (!allCollidedRaycasts[i].collider || allCollidedRaycasts[i].collider.gameObject.layer == 8)
//				continue;
//			//Vector3 objectHitPos = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - pointerRay.origin;
//			Vector3 objectHitPos = allCollidedRaycasts[i].point - pointerRay.origin;
//			if (objectHitPos.magnitude < shortestDistance || shortestDistance == -1.0) {
//				collidedRaycast = allCollidedRaycasts[i];
//				shortestDistance = objectHitPos.magnitude;
//			}
//		}
//		
//		return collidedRaycast;
//	}
//	
//	GameObject GetGimble(Ray pointerRay) {
//		RaycastHit[] allCollidedRaycasts = Physics.RaycastAll (pointerRay);
//		
//		GameObject gameobj = null;
//		float shortestDistance = -1.0f;
//		
//		// Find the collided object that's closest to the origin of the ray
//		for (int i=0; i < allCollidedRaycasts.Length; i++) {
//			if (!allCollidedRaycasts[i].collider
//				|| null == allCollidedRaycasts[i].collider.gameObject.GetComponent<GimbleSelection>()) //on top layer
//				continue;
//			//Vector3 objectHitPos = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - pointerRay.origin;
//			Vector3 objectHitPos = allCollidedRaycasts[i].point - pointerRay.origin;
//			if (objectHitPos.magnitude < shortestDistance || shortestDistance == -1.0) {
//				gameobj = allCollidedRaycasts[i].collider.gameObject;
//				shortestDistance = objectHitPos.magnitude;
//				gimbleHit = allCollidedRaycasts[i].collider.gameObject.transform.root.transform.position - allCollidedRaycasts[i].point;
//			}
//		}
//		return gameobj;
//	}
//	
//	void ButtonJustPressed(RaycastHit raycastHit) {
//
//		
//		if (!raycastHit.collider) {
//			return;
//		}
//		selected = raycastHit.collider.gameObject.transform.root.gameObject;
//		selectedObjectDistance =  raycastHit.distance;
//		selectedObjectHitPos = raycastHit.collider.gameObject.transform.root.transform.position - raycastHit.point;
//		selected.rigidbody.isKinematic = false;	
//		gimble.SetActiveRecursively(true);
//		//gimble.transform.position = raycastHit.point;
//	}
//		
//	void ButtonJustReleased(RaycastHit raycastHit) {
//		//gimble.SetActiveRecursively(false);
//		if (selected) {
//			selected.rigidbody.isKinematic = true;
//			selected = null;
//		}
//		gimble.SetActiveRecursively(false);
//		direction = Vector3.zero;
//	}
//	void ButtonPressed(Ray pointerRay) {
//		if (selected && direction != Vector3.zero) {
//			
//			selected.GetComponent<BlockSelection>().AddSelectedHighlight();
//			Vector3 diff = pointerRay.GetPoint(selectedObjectDistance)+selectedObjectHitPos - selected.transform.position;
//			diff = new Vector3(diff.x*direction.x, diff.y*direction.y, diff.z*direction.z);
//			Vector3 force = diff * springConstant - dampingConstant*selected.rigidbody.velocity;
//			if (force.magnitude > maxForce) {
//				force = maxForce*force.normalized;	
//			}
//			Debug.Log(diff);
//			Debug.Log(selectedObjectHitPos);
//			//selected.rigidbody.AddForce(force);
//			gimble.transform.position = selected.transform.position-selectedObjectHitPos;
//		}
//	}
//	
//	void ClearBlockHighlighting() {
//		Object[] objects = FindObjectsOfType(typeof(BlockSelection));
//		foreach (Object block in objects) {
//			BlockSelection blockSelection = block as BlockSelection;
//			blockSelection.RemoveHighlight();
//		}	
//	}
//}
//
