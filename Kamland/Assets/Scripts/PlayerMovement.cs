using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] bool drawDebugRaycasts = false;

    [Header("Movement Properties")]
    [SerializeField] int speed = 8;
    [SerializeField] float maxFallSpeed = -20f;

    [Header("Jump Properties")]
    [SerializeField] int jumpForce = 16;
    [SerializeField] float jumpReleaseMultiplier = 0.5f;
    [SerializeField] float coyoteDuration = 0.05f;
    [SerializeField] float jumpPressRememberDuration = 0.15f;

    [Header("Environment Check Properties")]
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;

    [Header("Status Flags")]
    public bool isOnGround;

    PlayerInput input;
    PlayerCombat playerCombat;
    BoxCollider2D boxCollider;
    Rigidbody2D rigidBody;

    float coyoteTime;
    float jumpPressRememberTime;

    float originalXScale;
    int direction = 1;

    float footOffsetXLeft;                                  //Left foot FROM players perspective
    float footOffsetXRight;                                 //Left foot FROM players perspective
    float footOffsetY;                                      //Diference between pos.y and colliderMinY

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerCombat = GetComponent<PlayerCombat>();
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        originalXScale = transform.localScale.x;

        footOffsetXLeft = boxCollider.offset.x + boxCollider.size.x * 0.5f;
        footOffsetXRight = boxCollider.offset.x - boxCollider.size.x * 0.5f;
        footOffsetY = boxCollider.offset.y - boxCollider.size.y * 0.5f;
    }

    void FixedUpdate()
    {
        PhysicsCheck();
        GroundMovement();
        MidAirMovement();
    }


    void PhysicsCheck()
    {
        isOnGround = false;

        if (rigidBody.velocity.y > 0)
            return;

        RaycastHit2D leftHit = RaycastWithOffset(new Vector2(footOffsetXLeft, footOffsetY), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightHit = RaycastWithOffset(new Vector2(footOffsetXRight, footOffsetY), Vector2.down, groundDistance, groundLayer);
        if (leftHit || rightHit)
            isOnGround = true;
    }

    void GroundMovement()
    {
        float xTargetVelocity = speed * input.horizontal;
        if (playerCombat.onCombat && isOnGround)
            xTargetVelocity = 0;
        else
        if (xTargetVelocity * direction < 0f)
            FlipCharacterDirection();

        // xTargetVelocity = Mathf.Lerp(rigidBody.velocity.x, xTargetVelocity, 0.9f);
        rigidBody.velocity = new Vector2(xTargetVelocity, rigidBody.velocity.y);
    }

    void MidAirMovement()
    {
        // if (playerCombat.onCombat)
        //     return;

        if (input.jumpPressed)
            jumpPressRememberTime = Time.time + jumpPressRememberDuration;
        if (isOnGround)
            coyoteTime = Time.time + coyoteDuration;

        if (jumpPressRememberTime > Time.time && coyoteTime > Time.time)
        {
            jumpPressRememberTime = 0;
            coyoteTime = 0;
            isOnGround = false;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        if (!isOnGround)
        {
            //Cut Y velocity if player isn't holding jump button anymore
            if (!input.jumpHeld)
            {
                if (rigidBody.velocity.y > 0)
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y * jumpReleaseMultiplier);
            }
        }

        //Player can't fall faster than maxFallSpeed
        if (rigidBody.velocity.y < maxFallSpeed)
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
    }

    void FlipCharacterDirection()
    {
        direction *= -1;
        footOffsetXLeft *= -1;
        footOffsetXRight *= -1;

        Vector3 scale = transform.localScale;
        scale.x = originalXScale * direction;
        transform.localScale = scale;
    }

    RaycastHit2D RaycastWithOffset(Vector2 offset, Vector2 rayDirection, float length, LayerMask layerMask)
    {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, layerMask);

        if (drawDebugRaycasts)
        {
            Color color = hit ? Color.red : Color.green;
            Debug.DrawLine(pos + offset, pos + offset + new Vector2(0f, -groundDistance), color);
        }
        return hit;
    }
}