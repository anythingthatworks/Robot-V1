using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public Rigidbody2D rb;

    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private LayerMask ground;  // A mask determining what is ground to the character
    [SerializeField] private Transform groundCheck;  // A position marking where to check if the player is grounded.
    [SerializeField] private float postDropPlatformReset = 1f; //time until platforms become solid again

    private bool canJump = false;
    private float groundedRadius = .4f; // Radius of the overlap circle to determine if grounded

    private Collider2D[] colliders; //list of colliders we use for jumping and dropping through platforms
    private List<Collider2D> downPlatforms = new List<Collider2D>(); //list of colliders whose effectors need to be reset so you can stop when jumping onto them
    private float dropTime = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        Move(rb, moveSpeed);

        canJump = CheckGround(groundCheck, groundedRadius, ground, rb);

        if (canJump && Input.GetAxisRaw("Jump")==1)
        {
            postDrop();
            Jump(rb, jumpForce);
            canJump = false;
        }


        if (Input.GetAxisRaw("Vertical") == -1)
        {
            Drop();
        }
        else if(Time.time-dropTime>postDropPlatformReset)
        {
            postDrop();
        }
    }

    // Horizontal movement function
    void Move(Rigidbody2D rb, float moveSpeed)
    {
        rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, rb.velocity.y);
    }

    bool CheckGround(Transform groundCheck, float groundedRadius, LayerMask ground, Rigidbody2D rb)
    {
        colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, ground);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject && rb.velocity.y <= 0) //2nd condition prevents one way platforms from giving another jump
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
        rb.AddForce(force);
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
                    Debug.Log("setting");
                    platform.GetComponent<PlatformEffector2D>().rotationalOffset = 180;
                    downPlatforms.Add(platform);
                }
            }
        }
    }

    //reset platforms after drop
    private void postDrop()
    {
        dropTime = float.MaxValue; //by setting it high we avoid tripping it again
        foreach (Collider2D platform in downPlatforms)
        {
            Debug.Log("reset");
            platform.GetComponent<PlatformEffector2D>().rotationalOffset = 0;
        }
        downPlatforms.Clear();
    }
}
