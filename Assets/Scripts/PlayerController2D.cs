using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public Rigidbody2D rb;

    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private LayerMask ground;  // A mask determining what is ground to the character
    [SerializeField] private LayerMask battery;  // A mask determining what is ground to the character
    [SerializeField] private Transform groundCheck;  // A position marking where to check if the player is grounded.
    [SerializeField] private float postDropPlatformReset = 1f; //time until platforms become solid again

    private LayerMask canJumpMask;
    private bool canJump = false;
    private float groundedRadius = .4f; // Radius of the overlap circle to determine if grounded

    private Collider2D[] colliders; //list of colliders we use for jumping and dropping through platforms
    private List<Collider2D> downPlatforms = new List<Collider2D>(); //list of colliders whose effectors need to be reset so you can stop when jumping onto them
    private float dropTime = 0;

    private GameObject carryingBattery = null;
    private Transform batteryHoldingPos; //position on sprite of box
    [SerializeField] private float batteryPickupRange = 1f; //range you can pick up box from
    private float pickUpTime = 0;
    [SerializeField]private float pickUpCooldown = 1f;

    //called once on startup
    private void Start()
    {
        batteryHoldingPos = transform.Find("HoldingPos");


        //layermask fuckery: https://answers.unity.com/questions/8715/how-do-i-use-layermasks.html
        canJumpMask = ground | battery;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        Move(rb, moveSpeed);

        canJump = CheckGround(groundCheck, groundedRadius, canJumpMask, rb);

        if (canJump && Input.GetAxisRaw("Vertical")==1)
        {
            PostDrop();
            Jump(rb, jumpForce);
            canJump = false;
        }

        /*high concept behind dropping
         *you switch the orientation of the one way stop on the platform
         * you store the platforms you did this for in downPlatforms list
         * eventually you reset all those platforms, either when they jump, or after enough time has passed
         */
        if (Input.GetAxisRaw("Vertical") == -1)
        {
            Drop();
        }
        else if(Time.time-dropTime>postDropPlatformReset)
        {
            PostDrop();
        }

        /*high concept behind picking up and putting down
         *you are creating a joint between the two objects
         * you want it on a cooldown so you don't drop it instantly
         * the cooldown timer is a little more complex but does less math
         * you're assigning a joint, and disabling one of the collisions
         * the battery actaully has two hit boxes, one that does the physics interactions
         * and another that we use to check if the palyer is standing inside the bgox
         * when you put the box down it doesn't re enable collisions until you stop stasnding in it
         * all of this is done on this side of the script to avoid complex interations between box and player
         */ 
        if (Input.GetKey("q") && Time.time - pickUpTime > pickUpCooldown)
        {
            pickUpTime = Time.time;
            if (carryingBattery==null)
            {
                PickUp();
            }
            else
            {
                PutDown();
            }
        }
    }

    // Horizontal movement function
    void Move(Rigidbody2D rb, float moveSpeed)
    {
        rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, rb.velocity.y);
    }

    bool CheckGround(Transform groundCheck, float groundedRadius, LayerMask canJumpMask, Rigidbody2D rb)
    {
        colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, canJumpMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.CompareTag("Battery") && !colliders[i].gameObject.GetComponent<BatteryMove>().getCarried())
            {
                return true;
            }
            else if (colliders[i].gameObject != gameObject && rb.velocity.y <= 0) //2nd condition prevents one way platforms from giving another jump
            {
                return true;
            }
        }
        return false;
    }

    // Successful juump movement function
    void Jump(Rigidbody2D rb, float jumpForce)
    {
        Vector2 force = new Vector2 (0, jumpForce);
        rb.velocity = force;
    }

    //dropping through platforms
    void Drop()
    {
        dropTime = Time.time;
        if (colliders.Length > 0)
        {
            foreach (Collider2D platform in colliders)
            {
                if (platform.GetComponent<PlatformEffector2D>())
                {
                    platform.GetComponent<PlatformEffector2D>().rotationalOffset = 180;
                    downPlatforms.Add(platform);
                }
            }
        }
    }

    //reset platforms after drop
    private void PostDrop()
    {
        dropTime = float.MaxValue; //by setting it high we avoid tripping it again
        foreach (Collider2D platform in downPlatforms)
        {
            platform.GetComponent<PlatformEffector2D>().rotationalOffset = 0;
        }
        downPlatforms.Clear();
    }

    private void PickUp()
    {
        Collider2D[] inRange = Physics2D.OverlapCircleAll(transform.position, batteryPickupRange);
        foreach (Collider2D thing in inRange)
        {
            if (thing.CompareTag("Battery"))
            {
                carryingBattery = thing.gameObject;
                break; //exit on the first battery to save time
            }
        }

        if(carryingBattery != null)
        {
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), carryingBattery.GetComponent<Collider2D>());
            carryingBattery.transform.position = batteryHoldingPos.transform.position;
            carryingBattery.GetComponent<FixedJoint2D>().enabled = true;
            carryingBattery.GetComponent<FixedJoint2D>().connectedBody = gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void PutDown()
    {
        carryingBattery.GetComponent<FixedJoint2D>().connectedBody = null;
        carryingBattery.GetComponent<FixedJoint2D>().enabled = false;
        carryingBattery = null;
    }

    //this is used to make batteries physical again after you pass out of them
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (carryingBattery == null && collision.gameObject.transform.root.CompareTag("Battery"))
        {
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision.gameObject.transform.root.GetComponent<Collider2D>(), false);
        }
    }
}
