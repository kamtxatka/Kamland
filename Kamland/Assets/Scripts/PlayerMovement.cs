using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] bool drawDebugRaycasts = false;            //Draw visible raycasts for debugging

    [Header("Movement Properties")]
    [SerializeField] float horizontalSpeed = 8.3f;              //Horizontal speed
    [SerializeField] float maxFallSpeed = -20.9f;               //Max speed char can fall at

    [Header("Jump Properties")]
    [SerializeField] float jumpSpeed = 15.7f;                   //Jump force
    [SerializeField] float jumpReleaseMultiplier = 0.5f;        //Ratio at which the vertical speed is cutted when releasing jump input
    [SerializeField] float coyoteDuration = 0.15f;              //Duration we consider player is STILL on ground
    [SerializeField] float jumpPressRememberDuration = 0.15f;   //Duration we consider player is STILL pressing jump input

    [Header("Environment Check Properties")]
    [SerializeField] float groundDistance = 0.2f;               //Distance to validate we are on ground (raycasts)
    [SerializeField] LayerMask groundLayer;                     //What layer can teh player walk on?

    [Header("Status Flags")]
    public bool isOnGround;                                     //Is player on ground

    PlayerInput input;
    PlayerCombat playerCombat;
    BoxCollider2D boxCollider;
    Rigidbody2D rigidBody;

    float coyoteTime;                                           //We consider player is on ground untill this time
    float jumpPressRememberTime;                                //We consider players is input jumping untill this time

    float originalXScale;                                       //For turning char
    int direction = 1;                                          //Current char dir. Right 1, Left -1

    float footOffsetXLeft;                                      //Left foot FROM players perspective
    float footOffsetXRight;                                     //Left foot FROM players perspective
    float footOffsetY;                                          //Diference between pos.y and colliderMinY

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerCombat = GetComponent<PlayerCombat>();
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        originalXScale = transform.localScale.x;

        //Left char foot is to the right of screen (player facing right)
        //Right char foot is to the left of the screen (player facing right)
        footOffsetXLeft = direction * boxCollider.offset.x + boxCollider.size.x * 0.5f;
        footOffsetXRight = direction * boxCollider.offset.x - boxCollider.size.x * 0.5f;
        footOffsetY = boxCollider.offset.y - boxCollider.size.y * 0.5f;
    }

    void FixedUpdate()
    {
        PhysicsCheck();
        HorizontalMovement();
        MidAirMovement();
    }

    /// <summary>
    /// Check if player is on ground + other physic checks in the future
    /// </summary>
    void PhysicsCheck()
    {
        isOnGround = false;

        if (rigidBody.velocity.y > 0f)
            return;

        RaycastHit2D leftHit = RaycastWithOffset(new Vector2(footOffsetXLeft, footOffsetY), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightHit = RaycastWithOffset(new Vector2(footOffsetXRight, footOffsetY), Vector2.down, groundDistance, groundLayer);
        if (leftHit || rightHit)
            isOnGround = true;
    }

    /// <summary>
    /// Horizontal Movement related behaviour
    /// </summary>
    void HorizontalMovement()
    {
        float xTargetVelocity = horizontalSpeed * input.horizontal;

        //Conditions to limit horizontal speed:
        //Attacking on ground
        if (playerCombat.onCombatAnimation && isOnGround)
            xTargetVelocity = 0f;

        //Conditions to be able to turn the character:
        //Not currently attacking
        if (!playerCombat.onCombatAnimation)
            if (xTargetVelocity * direction < 0f)
                FlipCharacterDirection();

        rigidBody.velocity = new Vector2(xTargetVelocity, rigidBody.velocity.y);
    }

    /// <summary>
    /// Vertical Movement related behaviour
    /// </summary>
    void MidAirMovement()
    {
        //Conditions to make jump easier
        if (input.jumpPressed)
            jumpPressRememberTime = Time.time + jumpPressRememberDuration;
        if (isOnGround)
            coyoteTime = Time.time + coyoteDuration;

        //Jump
        if (jumpPressRememberTime > Time.time && coyoteTime > Time.time)
        {
            jumpPressRememberTime = 0f;
            coyoteTime = 0f;
            isOnGround = false;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed);
        }

        //Cut Y velocity if player isn't holding jump button anymore
        if (!isOnGround && !input.jumpHeld)
        {
            if (rigidBody.velocity.y > 0f)
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y * jumpReleaseMultiplier);
        }

        //Player can't fall faster than maxFallSpeed
        if (rigidBody.velocity.y < maxFallSpeed)
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
    }

    void FlipCharacterDirection()
    {
        direction *= -1;
        footOffsetXLeft *= -1f;
        footOffsetXRight *= -1f;

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