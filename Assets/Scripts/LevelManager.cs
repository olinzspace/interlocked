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
	private int[] numSelectByPiece;

	// Use this for initialization
	void Start () {
		firstPieceLiberationTime = new TimeSpan(0);
		if (pen.GetComponent<PenGrab>()) {
			numSelectByPiece = new int[pen.GetComponent<PenGrab>().puzzlePieces.Length];
		} else {
			//FIX ME
			numSelectByPiece = new int[pen.GetComponent<MouseGrab>().puzzlePieces.Length];
		}
			
		sw = new Stopwatch();
		sw.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void IncrNumSelectEvents(int pieceIndex) {
		numSelectByPiece[pieceIndex]++;
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
			
			string numSelectByPieceStr = "";
			for (int i=0; i<numSelectByPiece.Length; i++) {
				numSelectByPieceStr += numSelectByPiece[i];
				if (i != numSelectByPiece.Length) {
					numSelectByPieceStr += ", ";
				}
			}
			for (int i=numSelectByPiece.Length; i<8; i++) {
				numSelectByPieceStr += ", --";
			}
			
			if (!logExists) {
				writer.Write (	"UserID, " + 
							  	"Date, " +
								"LevelId, " +
								"TotalElapsedTime, " + 
								"NumSelectionEvents, " +
								"NumMouseEvents, " + 
								"FirstPieceLiberationTime, " + 
								"Piece1Selections, " +
								"Piece2Selections, " +
								"Piece3Selections, " +
								"Piece4Selections, " +
								"Piece5Selections, " +
								"Piece6Selections, " +
								"Piece7Selections, " +
								"Piece8Selections" + "\n");
								
			}
			writer.Write (	userId + ", " + 
							dateString + ", " + 
							Application.loadedLevelName + ", " + 
							FormattedTimeString(totalElapsedTime) + ", " +
							numSelectEvents + ", " +
							numMouseEvents + ", " + 
							FormattedTimeString(firstPieceLiberationTime) + ", " +
							numSelectByPieceStr + "\n");
		}
		
		Application.LoadLevel (nextLevel);
	}
	
	private String FormattedTimeString (TimeSpan ts) {
		return String.Format ("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
	}
}