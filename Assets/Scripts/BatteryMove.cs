using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryMove : MonoBehaviour
{
    [SerializeField]  float gridSpacing = 10;
    float spacing;
    Rigidbody2D body;
    [SerializeField] float stoppingSpeed = .05f;
    private Collision2D ground = null;
    private float neccesaryGroundTime = 1f;
    private float groundTime = 0;
    private List<Collision2D> hits = new List<Collision2D>();

    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
        spacing = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().getGridSpace() ;
    }

    // Update is called once per frame
    /*high concept
     *you check if the boxs is moving
     * youi check if it's on the ground
     * if it's slow enough snap it to the neerest grid spot
     * there is a timer while it's on the ground in order to make sure that you don't stop instantly if you hit something
     * or hit something then slide off
     * you freeze the position in order to not move the box when you step on it
     * grid spots are stoersd in the game manager cause why not
     * you use a list of objects to check if it's impacting ground to check if it's in midair
     * you know it's being carried if the join is enabled so you use that to check
     * Mathf. round only works to integers which is why there is some janky math there
     */ 
    void Update()
    {
        if(!getCarried())
        {
            if (body.velocity.y < stoppingSpeed && hits.Count >0)
            {
                groundTime += Time.deltaTime;
            }
            if (groundTime > neccesaryGroundTime)
            {
                groundTime = 0;
                Vector3 pos = transform.position;
                pos.x = pos.x / spacing;
                pos.x = Mathf.Round(pos.x) * spacing;
                transform.position = pos;
                body.velocity = new Vector2(0, body.velocity.y);
                body.constraints = RigidbodyConstraints2D.FreezePositionX;
            }
        }
        else
        {
            body.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            hits.Add(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            hits.Remove(collision);
        }
    }

    public bool getCarried()
    {
        return gameObject.GetComponent<FixedJoint2D>().enabled;
    }
}
