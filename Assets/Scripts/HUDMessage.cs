using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDMessage : MonoBehaviour {

    public Transform target;
    public Transform hmd;
    public float smoothing = 5f;
    
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Lerp(transform.position, target.position, smoothing * Time.deltaTime);
        transform.LookAt(hmd);
        transform.Rotate(0, 180, 0);
    }
}
