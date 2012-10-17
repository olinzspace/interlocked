using UnityEngine;
using System.Collections;

public class BlockSelection : MonoBehaviour {
	public Material baseMaterial, selectedMaterial, hoverMaterial;
	
	// Use this for initialization
	void Start () {
		RemoveHighlight();
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	public void AddHoverHighlight() {
		gameObject.renderer.material = hoverMaterial;
	}
	
	public void AddSelectedHighlight() {
		gameObject.renderer.material = selectedMaterial;
	}
	
	public void RemoveHighlight() {
		gameObject.renderer.material = baseMaterial;
	}
}
