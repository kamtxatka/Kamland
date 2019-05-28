using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//We make sure this monoB is executed before any other one
[DefaultExecutionOrder(-100)]
public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public int horizontal;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool attack;

    [SerializeField] KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] KeyCode rightKey = KeyCode.RightArrow;
    [SerializeField] KeyCode jumpKey = KeyCode.A;
    [SerializeField] KeyCode attackKey = KeyCode.S;

    bool readyToClear;


    void Update()
    {
        ClearInput();
        ProcessInputs();

        horizontal = Mathf.Clamp(horizontal, -1, 1);
    }

    //Inputs will be used in playerMovement's fixedUpdate. Set them ready to clear
    void FixedUpdate()
    {
        readyToClear = true;
    }

    //Will only clear inputs if we've gone throught an iteration of fixed update
    void ClearInput()
    {
        if (!readyToClear)
            return;

        horizontal = 0;
        jumpPressed = false;
        jumpHeld = false;
        attack = false;

        readyToClear = false;
    }

    void ProcessInputs()
    {
        if (Input.GetKey(leftKey))
            horizontal -= 1;
        if (Input.GetKey(rightKey))
            horizontal += 1;

        jumpPressed = Input.GetKeyDown(jumpKey);
        jumpHeld = Input.GetKey(jumpKey);

        attack = Input.GetKeyDown(attackKey);
    }
}