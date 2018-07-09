using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour {
    
    public float DestroyTime = 1f;
    public Vector3 Offset = new Vector3(0, 5, 0);

	// Use this for initialization
	void Start ()
    {
        Destroy(gameObject, DestroyTime);

        transform.localPosition += Offset;
	}
}
