﻿using UnityEngine;
using System.Collections;

public class Unity5test : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0)) GetComponent<Rigidbody>().AddTorque(0,0,-100);
	}
}
