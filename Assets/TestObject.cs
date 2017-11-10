using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MyList.instance.t.Add(transform);
	}
	
	
}
