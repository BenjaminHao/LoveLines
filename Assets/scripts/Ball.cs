using UnityEngine;

public class Ball : MonoBehaviour 
{
    public int value;
    public int power;
    public Rigidbody2D rb;
    public bool readyToUpgrade = false;
    public Vector2 upgradePos;
    // Use this for initialization
    void Start()
    {
        rb.velocity = new Vector2(power, power);
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToUpgrade) 
            ReadyToUpgrade(upgradePos);
        else
            // fixed speed
            rb.velocity = power * (rb.velocity.normalized);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ball" && readyToUpgrade)
        {
            Destroy(gameObject);
        }
    }

    private void ReadyToUpgrade(Vector2 pos)
    {
        transform.position = Vector2.MoveTowards(new Vector2(transform.position.x, transform.position.y),
                                                           pos, 3 * Time.deltaTime);
    }
}
