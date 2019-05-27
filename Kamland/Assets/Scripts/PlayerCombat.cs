using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [HideInInspector] public bool wantToAttack;                         //Input attack (+ holds a bit for easy control)
    [HideInInspector] public int comboStep = 1;                         //What attack would we going to use next
    [HideInInspector] public bool onCombatAnimation;                    //Are we in any attack animation?
    [HideInInspector] public bool onGroundCombat;                       //Player attacking... on ground?
    [HideInInspector] public bool onAirCombat;                          //Player attacking... on air?

    [SerializeField] float comboLength = 2f;                            //Self explanatory
    [SerializeField] float timeBetweenAttacks = 0.5f;                   //Self explanatory
    [SerializeField] float bonusTimeToEndComboRelation = 0.5f;          //Time to reset combo after an attack (relative to time between attacks)
    [SerializeField] float attackRememberDuration = 0.1f;               //Time to hold attack=true after input (for easy control)

    Animator animator;
    PlayerCombatMachine playerCombatMachine;
    PlayerInput playerInput;
    PlayerMovement playerMovement;

    float nextAttakTime;                                                //When will the player be able to attack again?
    float comboEndTime;                                                 //When will combo reset?
    float attackRememberTime;                                           //We want to attack up until this very moment

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();

        onCombatAnimation = false;
        onGroundCombat = false;
        onAirCombat = false;
        comboStep = 1;
    }

    void Start()
    {
        playerCombatMachine = animator.GetBehaviour<PlayerCombatMachine>();
        playerCombatMachine.playerCombat = this;
    }

    /// <summary>
    /// UpdateCombos is called from playerAnimation component right before updating animator.
    /// </summary>
    public void UpdateCombos()
    {
        //2 conditions to attack
        //Input
        //It has been enough time since last attack
        if (playerInput.attack)
            attackRememberTime = Time.time + attackRememberDuration;

        wantToAttack = (attackRememberTime > Time.time && Time.time > nextAttakTime);

        //We are sure player will attack. on ground - on air??
        if (wantToAttack)
        {
            if (playerMovement.isOnGround)
                onGroundCombat = true;
            else
                onAirCombat = true;
        }

        //Reset combo if time for combo has ended
        if (Time.time > comboEndTime)
            comboStep = 1;
    }

    public void OnStateMachineEnter()
    {
        onCombatAnimation = true;
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
        comboEndTime = nextAttakTime + timeBetweenAttacks * bonusTimeToEndComboRelation;
    }

    public void OnStateMachineExit()
    {
        onCombatAnimation = false;
        onGroundCombat = false;
        onAirCombat = false;
    }
}