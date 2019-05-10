using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [HideInInspector] public bool onCombo;
    [HideInInspector] public int comboStep = 1;
    [HideInInspector] public bool attack;

    [SerializeField] float comboLength = 2;
    [SerializeField] float timeBetweenAttacks = 0.5f;              //Time (seconds) between each attack

    PlayerInput input;
    PlayerMovement playerMovement;
    Animator animator;
    PlayerCombatMachine playerCombatMachine;

    float nextAttakTime;
    float comboEndTime;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        onCombo = false;
        comboStep = 1;
    }

    void Start()
    {
        playerCombatMachine = animator.GetBehaviour<PlayerCombatMachine>();
        playerCombatMachine.playerCombat = this;
    }

    public void UpdateCombos()
    {
        //3 conditions to attack
        //Input
        //On ground
        //It has been enough time since last attack
        attack = (input.attack && playerMovement.isOnGround && Time.time > nextAttakTime);

        //Reset combo if time for combo has ended
        if (Time.time > comboEndTime)
            comboStep = 1;
    }

    public void OnStateMachineEnter()
    {
        onCombo = true;
        comboEndTime = nextAttakTime + timeBetweenAttacks;
    }

    public void OnStateEnter()
    {
        //Iterate combo
        comboStep++;
        if (comboStep > comboLength)
            comboStep = 1;

        //Reset timers for next attack and combo end
        nextAttakTime = Time.time + timeBetweenAttacks;
        comboEndTime = nextAttakTime + timeBetweenAttacks;
    }

    public void OnStateMachineExit()
    {
        onCombo = false;
    }
}