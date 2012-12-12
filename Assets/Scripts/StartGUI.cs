using UnityEngine;
using System.Collections;

public class StartGUI : MonoBehaviour {

	private string prevUserid, userid;
	private string defaultUseridText = "Enter User Name/ID";
	private bool error = false;
	private string errorMessage;
	private GUIStyle titleStyle, errorStyle;
	
	public void Start () {
		userid = prevUserid = defaultUseridText;
		
		titleStyle = new GUIStyle();
		titleStyle.fontSize = 48;
		
		errorStyle = new GUIStyle();
		errorStyle.fontSize = 18;
	}
	
	public void OnGUI () {
		// Center components on screen
		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
	    GUILayout.FlexibleSpace();
	    GUILayout.BeginHorizontal();
	    GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		
		
		// Title
		GUILayout.Label("zSpace Interlocked", titleStyle);
		GUILayout.Space(20);
		
		// Userid textbox
		prevUserid = userid;
		userid = GUILayout.TextField(userid, 40, GUILayout.Width(400), GUILayout.Height(50));
		if (!userid.Equals(prevUserid))
			error = false;
		GUILayout.Space(10);
		
		// Level options
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("3D", GUILayout.Width(190), GUILayout.Height(50))) {
			if (userid.Equals("") || userid.Equals(defaultUseridText)) {
				error = true;
				errorMessage = "Invalid user name/ID";
			} else {
				LevelManager.userId = userid;
				Application.LoadLevel(1);			
			}
		}
		GUILayout.Space(20);
		if(GUILayout.Button("2D", GUILayout.Width(190), GUILayout.Height(50))) {
			if (userid.Equals("") || userid.Equals(defaultUseridText)) {
				error = true;
				errorMessage = "Invalid user name/ID";
			} else {
				LevelManager.userId = userid;
				Application.LoadLevel(7);
			}
		}
		GUILayout.EndHorizontal();
		
		// Error message (if any)
		if (error) {
			GUILayout.Space(20);
			GUILayout.Label(errorMessage, errorStyle);
		}
		
		
		// Center components on screen
		GUILayout.EndVertical();
	    GUILayout.FlexibleSpace();
	    GUILayout.EndHorizontal();
	    GUILayout.FlexibleSpace();
	    GUILayout.EndArea();
	}
}