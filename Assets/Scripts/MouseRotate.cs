using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class MouseRotate : MonoBehaviour {
	public GameObject levelManager;
	
	public float rotationScale = 10;
	public bool withKey = false;
	
	private Vector2 startMousePos;
	private float oldMouseX, oldMouseY;
	private bool altKeyIsDown = false;
	private bool altKeyWasDown = false;
	private int pauseTimeToRegisterMouseEvent = 500; // In milliseconds
	private Stopwatch mousePauseSw;
	private int i; // For debugging purposes
	
	// Use this for initializationf
	void Start () {
		oldMouseY = -Input.GetAxis ("Mouse Y");
		oldMouseX = -Input.GetAxis ("Mouse X");
		mousePauseSw = new Stopwatch();
		i = 0;
	}
	
	// Update is called once per frame
	void Update () {
		altKeyIsDown = Input.GetKey (KeyCode.LeftAlt);
		
		float mouseX = -Input.GetAxis ("Mouse X");
		float mouseY = -Input.GetAxis ("Mouse Y");
		float deltaMouseX = Mathf.Round ((mouseX - oldMouseX)/0.1f) * 0.1f;
		float deltaMouseY = Mathf.Round ((mouseY - oldMouseY)/0.1f) * 0.1f;
		
//		Debug.Log (new Vector4(deltaMouseX, oldDeltaMouseX, oldDeltaMouseY, i));
		
		if (!withKey || altKeyIsDown) {
			transform.RotateAround(Vector3.zero, -transform.TransformDirection(Vector3.up), mouseX*rotationScale);
			transform.RotateAround(Vector3.zero, -transform.TransformDirection(Vector3.left), mouseY*rotationScale);
		}
		
		// Logging mouse movements
		if (withKey) {
			if (altKeyIsDown && !altKeyWasDown) {
				levelManager.GetComponent<LevelManager>().IncrNumMouseEvents();
			}
		} else {
			if (deltaMouseX == 0.0f && deltaMouseY == 0.0f) {
				if (!mousePauseSw.IsRunning) {
					mousePauseSw.Start ();
				}
				
			} else if (mousePauseSw.IsRunning) {
				mousePauseSw.Stop ();
				if (mousePauseSw.ElapsedMilliseconds >= pauseTimeToRegisterMouseEvent) {
					levelManager.GetComponent<LevelManager>().IncrNumMouseEvents();
					// Code for debugging purposes
					i++;
					//				UnityEngine.Debug.Log ("This is the " + i + "th mouse event");				
				}
				
				mousePauseSw.Reset ();
			}
		}
		
		oldMouseX = mouseX;
		oldMouseY = mouseY;
		altKeyWasDown = altKeyIsDown;
	}
}
