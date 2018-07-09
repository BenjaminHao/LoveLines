using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowEffect : MonoBehaviour {

    public Transform target;
    public float MinModifier = 7;
    public float MaxModifier = 11;

    Vector3 _velocity = Vector3.zero;
    bool _isFollowing = false;

    public void StartFollowing()
    {
        _isFollowing = true;
    }

	// Update is called once per frame
	void Update () 
    {
        if (_isFollowing)
        {
            transform.position = Vector3.SmoothDamp(transform.position, target.position, ref _velocity, Time.deltaTime * Random.Range(MinModifier, MaxModifier));
        }
	}
}
