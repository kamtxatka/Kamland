using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rigidBody;
    PlayerInput playerInput;
    PlayerMovement playerMovement;
    PlayerCombat playerCombat;

    readonly int hVelocityParamID = Animator.StringToHash("hVelocity");
    readonly int vVelocityParamID = Animator.StringToHash("vVelocity");

    readonly int groundedParamID = Animator.StringToHash("grounded");

    readonly int attackParamID = Animator.StringToHash("isGonnaAttack");
    readonly int comboStepParamID = Animator.StringToHash("comboStep");

    readonly int hitParamID = Animator.StringToHash("hit");
    readonly int deathParamID = Animator.StringToHash("death");


    void Awake()
    {
        anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        //Character velocity
        float inputHorizontal = Mathf.Abs(playerInput.horizontal);
        anim.SetFloat(hVelocityParamID, inputHorizontal);
        anim.SetFloat(vVelocityParamID, rigidBody.velocity.y);

        //Mid Air State
        anim.SetBool(groundedParamID, playerMovement.grounded);

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