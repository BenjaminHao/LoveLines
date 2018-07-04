using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoEffect : MonoBehaviour {

    private float timeBtwSpawns;
    public float startTimeBtwSpawns;

    public GameObject echo;
    private Ball ball;

	// Use this for initialization
	void Start () {
        ball = GetComponent<Ball>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if (ball.GetComponent<Rigidbody2D>().velocity.magnitude > new Vector2(20,20).magnitude)
        {
            if(timeBtwSpawns <= 0)
            {
                // spawn echo game object
                GameObject instance = (GameObject)Instantiate(echo, transform.position, Quaternion.identity);
                Destroy(instance, 0.5f);
                timeBtwSpawns = startTimeBtwSpawns;
            }
            else
            {
                timeBtwSpawns -= Time.unscaledDeltaTime;
            }
        }
	}
}
