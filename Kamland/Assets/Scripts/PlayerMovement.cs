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
    [SerializeField] float jumpingImpulseDuration = 0.1f;       //Duration we set yspeed to jumpspeed when jumping
    [SerializeField] float jumpReleaseMultiplier = 0.5f;        //Ratio at which the vertical speed is cutted when releasing jump input
    [SerializeField] float coyoteDuration = 0.15f;              //Duration we consider player is STILL on ground
    [SerializeField] float jumpPressRememberDuration = 0.15f;   //Duration we consider player is STILL pressing jump input

    [Header("Environment Check Properties")]
    [SerializeField] float groundDistance = 0.2f;               //Distance to validate we are on ground (raycasts)
    [SerializeField] LayerMask groundLayer;                     //What layer can teh player walk on?

    [Header("Status Flags")]
    public bool grounded;                                       //Is player on ground
    public bool jumping;                                        //Is player jumping? (impulses cancells it)
    public bool impulsed;                                       //Char has been impulsed (true untill touches ground) 

    BoxCollider2D boxCollider;
    Rigidbody2D rigidBody;
    PlayerInput playerInput;
    PlayerCombat playerCombat;

    Vector2 targetImpulse;                                      //Any impulse player recieves (being hit, air attack, etc)

    float jumpingImpulseTime;                                   //After this time we wont set yspeed to jumpspeed anymore
    float coyoteTime;                                           //We consider player is on ground untill this time
    float jumpPressRememberTime;                                //We consider players is input jumping untill this time

    float originalXScale;                                       //For turning char
    int direction;                                              //Current char dir. Right 1, Left -1

    float footOffsetXLeft;                                      //Left foot FROM players perspective
    float footOffsetXRight;                                     //Left foot FROM players perspective
    float footOffsetY;                                          //Diference between pos.y and colliderMinY

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerCombat = GetComponent<PlayerCombat>();

        originalXScale = transform.localScale.x;
        direction = 1;

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
        ImpulseMovement();
    }

    /// <summary>
    /// Check grounded, falling and jumping state.
    /// </summary>
    void PhysicsCheck()
    {
        grounded = false;

        if (rigidBody.velocity.y > 0f)
            return;

        RaycastHit2D leftHit = RaycastWithOffset(new Vector2(footOffsetXLeft, footOffsetY), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightHit = RaycastWithOffset(new Vector2(footOffsetXRight, footOffsetY), Vector2.down, groundDistance, groundLayer);
        if (leftHit || rightHit)
        {
            grounded = true;
            jumping = false;
            impulsed = false;
        }
    }

    /// <summary>
    /// Horizontal Movement related behaviour.
    /// </summary>
    void HorizontalMovement()
    {
        float xTargetVelocity = horizontalSpeed * playerInput.horizontal;

        //Conditions to avoid horizontal movement:
        //Performing a grounded attack
        //Say we started attacking on air. We could still move horizontallly when we touch the ground
        if (playerCombat.onGroundCombat)
            xTargetVelocity = 0f;

        //Conditions to be able to turn the character:
        //Not currently attacking
        if (!playerCombat.attacking)
            if (xTargetVelocity * direction < 0f)
                FlipCharacterDirection();

        rigidBody.velocity = new Vector2(xTargetVelocity, rigidBody.velocity.y);
    }

    /// <summary>
    /// Vertical Movement related behaviour.
    /// </summary>
    void MidAirMovement()
    {
        bool willJump = false;
        Vector2 targetVelocity = rigidBody.velocity;

        //Conditions to jump (with timers to make easier controls)
        //1 Input
        //2 OnGround
        if (playerInput.jumpPressed)
            jumpPressRememberTime = Time.time + jumpPressRememberDuration;
        if (grounded)
            coyoteTime = Time.time + coyoteDuration;

        willJump = jumpPressRememberTime > Time.time && coyoteTime > Time.time;

        //Conditions to cancel jump
        //Attacking
        if (playerCombat.attacking)
            willJump = false;

        //Jump
        if (willJump)
        {
            jumpingImpulseTime = Time.time + jumpingImpulseDuration;

            jumpPressRememberTime = 0f;
            coyoteTime = 0f;
            grounded = false;
            jumping = true;
        }
        if (jumpingImpulseTime > Time.time)
            targetVelocity.y = jumpSpeed;

        //Cut vSpeed >= 0 when:
        //Player jumped but isn't holding jump button anymore
        //Player has recieved an impulse
        if (targetVelocity.y > 0f)
        {
            bool cutvSpeed = (jumping && !playerInput.jumpHeld) || impulsed;
            if (cutvSpeed)
                targetVelocity.y = rigidBody.velocity.y * jumpReleaseMultiplier;
        }

        //Player can't fall faster than maxFallSpeed
        if (targetVelocity.y < maxFallSpeed)
            targetVelocity.y = maxFallSpeed;

        //Finally lets apply the wanted velocity
        rigidBody.velocity = targetVelocity;
    }

    /// <summary>
    /// Impulse (vector2) related behaviour. e.g. Being hit, air attack, etc.
    /// </summary>
    void ImpulseMovement()
    {
        if (this.targetImpulse == Vector2.zero)
            return;

        impulsed = true;
        jumping = false;

        Vector2 impulseApplied = rigidBody.velocity;

        if (targetImpulse.x != 0)
            impulseApplied.x = targetImpulse.x;
        if (targetImpulse.y != 0)
            impulseApplied.y = targetImpulse.y;

        rigidBody.velocity = impulseApplied;

        targetImpulse = Vector2.zero;
    }

    /// <summary>
    /// Stacks an Impulse (vector2) which will be aplied on the next fixedUpdate.
    /// </summary>
    public void ReceiveSpeedImpulse(Vector2 impulseVector)
    {
        this.targetImpulse = impulseVector;
    }

    /// <summary>
    /// Flips Char + swaps foot offset for ground detection.
    /// </summary>
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