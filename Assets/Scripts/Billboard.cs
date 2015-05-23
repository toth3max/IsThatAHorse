using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		this.transform.rotation = Quaternion.Euler(0f,RotateCamera.instance.yDirection,0f);



	}
}
