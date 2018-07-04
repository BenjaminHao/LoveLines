using UnityEngine;

public class Ball : MonoBehaviour 
{
    public int value;
    public int power;
    public Rigidbody2D rb;
    public bool readyToUpgrade = false;
    public bool touched = false;
    public GameObject upgradeBall;
    float timeStamp;
    // Use this for initialization
    void Start()
    {
        rb.velocity = new Vector2(power, power);
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToUpgrade) 
            ReadyToUpgrade(upgradeBall);
        else
            // fixed speed
            rb.velocity = power * (rb.velocity.normalized);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        touched |= (collision.collider.tag == "Ball" && readyToUpgrade);
        if (collision.collider.tag == "Line" && readyToUpgrade)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }

    private void ReadyToUpgrade(GameObject anotherBall)
    {
        timeStamp = Time.time;
        anotherBall = upgradeBall;
        Vector2 direction = -(transform.position - anotherBall.transform.position).normalized;
        rb.velocity = new Vector2(direction.x, direction.y) * 50f * (Time.time / timeStamp);
    }
}
