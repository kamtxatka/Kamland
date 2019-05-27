using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    PlayerMovement playerMovement;
    PlayerCombat playerCombat;
    Rigidbody2D rigidBody;
    PlayerInput input;
    Animator anim;

    readonly int hMovementParamID = Animator.StringToHash("hMovement");
    readonly int movingParamID = Animator.StringToHash("isMoving");

    readonly int groundParamID = Animator.StringToHash("isOnGround");
    readonly int airParamID = Animator.StringToHash("verticalVelocity");

    readonly int attackParamID = Animator.StringToHash("isGonnaAttack");
    readonly int comboStepParamID = Animator.StringToHash("comboStep");

    readonly int hitParamID = Animator.StringToHash("hit");
    readonly int deathParamID = Animator.StringToHash("death");


    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        rigidBody = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //Grounded State
        float inputHorizontal = Mathf.Abs(input.horizontal);
        anim.SetFloat(hMovementParamID, inputHorizontal);
        anim.SetBool(movingParamID, (inputHorizontal != 0) ? true : false);

        //Mid Air State
        anim.SetBool(groundParamID, playerMovement.isOnGround);
        anim.SetFloat(airParamID, rigidBody.velocity.y);

        //Combat
        playerCombat.UpdateCombat();
        anim.SetBool(attackParamID, playerCombat.isGonnaAttack);
        anim.SetInteger(comboStepParamID, playerCombat.comboStep);
    }

    public void AnimPlayerHit()
    {
        anim.SetTrigger(hitParamID);
    }

    public void AnimPlayerDeath()
    {
        anim.SetTrigger(deathParamID);
    }
}