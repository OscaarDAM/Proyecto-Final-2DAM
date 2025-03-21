using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private new Rigidbody2D rigidbody2D;
    private Vector2 direction;
    private Collider2D collider;
    private Collider2D[] results;
    public float moveSpeed = 3f;
    public float jumpForce = 2f;
    private bool isGrounded;
    private bool climbing;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        results =  new Collider2D[4];
    }

    private void CheckCollision()
    {

        isGrounded = false;
        climbing = false;

        Vector2 size = collider.bounds.size;
        size.y += 0.01f;
        size.x /= 2f;

        int amount = Physics2D.OverlapBoxNonAlloc(transform.position, size, 0f, results);

        for (int i = 0; i < amount; i++)
        {
            GameObject hit = results[i].gameObject;

            if (hit.layer == LayerMask.NameToLayer("Suelo"))
            {
                isGrounded = hit.transform.position.y < (transform.position.y - 0.5f);
                
                Physics2D.IgnoreCollision(collider, results[i], !isGrounded);

            } else if (hit.layer == LayerMask.NameToLayer("Ladder"))
            {
                climbing = true;
            }
        }

    }

    private void Update()
    {

        CheckCollision();

        if(climbing)
        {

            direction.y = Input.GetAxis("Vertical") * moveSpeed;

            if (direction.y == 0)
        {
            direction.y = -1f;
        }

        }

        else if (Input.GetButtonDown("Jump") && isGrounded)
        {
            direction = Vector2.up * jumpForce;

        } else
        {
            direction += Physics2D.gravity * Time.deltaTime;
        }

        direction.x = Input.GetAxis("Horizontal") * moveSpeed;

        if (isGrounded)
        {
            direction.y = Mathf.Max(direction.y, -1f);
        }
        

        if (direction.x > 0)
        {
            transform.eulerAngles = Vector3.zero;
             
        } else if (direction.x < 0)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }

    }

    private void FixedUpdate()
    {
        rigidbody2D.MovePosition(rigidbody2D.position + direction * Time.fixedDeltaTime);
    }
}
