using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class LevelManager : MonoBehaviour {
	public int nextLevel;
	public GameObject pen;
	private string filename = "userdata.csv";
	private string userIdLocation = "userId.txt";

	private Stopwatch sw;
	private String userId;
	private int numSelectEvents, numMouseEvents;
	private TimeSpan firstPieceLiberationTime;
	
	// Use this for initialization
	void Start () {
		numSelectEvents = 0;
		numMouseEvents = 0;
		firstPieceLiberationTime = new TimeSpan(0);
		
		sw = new Stopwatch();
		sw.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void IncrNumSelectEvents() {
		numSelectEvents++;
	}
	
	public void IncrNumMouseEvents() {
		numMouseEvents++;
	}
	
	public void SetFirstPieceLiberationTime() {
		if (firstPieceLiberationTime.Ticks == 0) { // Has it not been set yet?
			firstPieceLiberationTime = sw.Elapsed;
			UnityEngine.Debug.Log ("Time to liberate first piece: " + FormattedTimeString (firstPieceLiberationTime));
		} else {
			UnityEngine.Debug.Log ("WARNING: FIRST PIECE WAS ALREADY LIBERATED!");
		}
	}
	
	public void LevelFinished () {
		// Get userId
		userId = "FakeUser";
		try {
			using (StreamReader reader = new StreamReader (userIdLocation)) {
				userId = reader.ReadLine ();
			}
			if (userId == null || userId.Equals("")) throw new FileNotFoundException();
		} catch (FileNotFoundException) {
			userId = "------";
		}
		
		// Get today's date
		string dateString = DateTime.Now.ToString ("yyyy-MM-dd");
		
		// Get total ellapsed time
		sw.Stop();
		TimeSpan totalElapsedTime = sw.Elapsed;
		UnityEngine.Debug.Log ("Total time for level: " + FormattedTimeString(totalElapsedTime));
		UnityEngine.Debug.Log ("Total number of selections: " + numSelectEvents);
		
		bool logExists = File.Exists (filename);
		using (StreamWriter writer = new StreamWriter (filename, true)) {
			if (!logExists) {
				writer.Write (	"UserID, " + 
							  	"Date, " +
								"LevelId, " +
								"TotalElapsedTime, " + 
								"NumSelectionEvents, " +
								"NumMouseEvents, " + 
								"FirstPieceLiberationTime" + "\n");
								
			}
			writer.Write (	userId + ", " + 
							dateString + ", " + 
							Application.loadedLevelName + ", " + 
							FormattedTimeString(totalElapsedTime) + ", " +
							numSelectEvents + ", " +
							numMouseEvents + ", " + 
							FormattedTimeString(firstPieceLiberationTime) + "\n");
		}
		
		Application.LoadLevel (nextLevel);
	}
	
	private String FormattedTimeString (TimeSpan ts) {
		return String.Format ("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
	}
}