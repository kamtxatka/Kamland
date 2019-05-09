using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [HideInInspector] public bool onCombo;
    [HideInInspector] public int comboStep = 1;
    [HideInInspector] public bool attack;

    [SerializeField] float comboLength = 3;
    [SerializeField] float timeBetweenAttacks = 0.5f;              //Time (seconds) between each attack

    PlayerInput input;
    PlayerMovement playerMovement;
    Animator animator;
    PlayerCombatMachine playerCombatMachine;

    bool comboAdded;                                                //Will we go to the next combo state?
    float attackTime;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        onCombo = false;
        comboAdded = false;
    }

    void Start()
    {
        playerCombatMachine = animator.GetBehaviour<PlayerCombatMachine>();
        playerCombatMachine.playerCombat = this;
    }

    public void UpdateCombos()
    {
        attack = input.attack;

        if (!playerMovement.isOnGround)
            attack = false;

        if (attack && onCombo && !comboAdded)
        {
            comboStep++;
            if (comboStep > comboLength)
                comboStep = 1;

            // comboStep = comboStep == 1 ? 2 : 1;
            comboAdded = true;
        }
    }

    public void OnStateMachineEnter()
    {
        onCombo = true;
    }

    public void OnStateEnter()
    {
        comboAdded = false;
    }

    public void OnStateMachineExit()
    {
        onCombo = false;
        comboStep = 1;
    }
}