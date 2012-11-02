using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class LevelManager : MonoBehaviour {
	public int nextLevel;
	private string fileName = "userdata.csv";	

	private Stopwatch sw;
	
	// Use this for initialization
	public void Start () {
		sw = new Stopwatch();
		sw.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void LevelFinished () {
		
		// Get today's date
		string dateString = DateTime.Now.ToString ("yyyy-MM-dd");
		
		// Get total ellapsed time
		sw.Stop();
		TimeSpan ts = sw.Elapsed;
		string totalTime = String.Format ("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
		UnityEngine.Debug.Log ("Total time for level: " + totalTime);
		
		bool logExists = File.Exists (fileName);
		using (StreamWriter writer = new StreamWriter (fileName, true)) {
			if (!logExists) {
				writer.Write ("UserName, Date, LevelId, TotalElapsedTime\n");
			}
			writer.Write ("GenericUser, " + dateString + ", " + Application.loadedLevelName + ", " + totalTime + "\n");
		}
		
		Application.LoadLevel (nextLevel);
	}
}