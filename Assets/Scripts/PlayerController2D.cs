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

    private bool canJump = false;
    private float groundedRadius = .4f; // Radius of the overlap circle to determine if grounded

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move(rb, moveSpeed);

        if (!canJump)
        {
            canJump=CheckGround(groundCheck, groundedRadius, ground, rb);
        }

        if (canJump && Input.GetAxisRaw("Jump")==1)
        {
            Jump(rb, jumpForce);
            canJump = false;
        }

    }

    // Horizontal movement function
    void Move(Rigidbody2D rb, float moveSpeed)
    {
        rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, rb.velocity.y);
    }

    bool CheckGround(Transform groundCheck, float groundedRadius, LayerMask ground, Rigidbody2D rb)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, ground);
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
}
