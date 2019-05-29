using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [HideInInspector] public bool isGonnaAttack;                        //Will player attack this update cycle (for sure)?
    [HideInInspector] public int comboStep = 1;                         //What attack would we going to use next
    [HideInInspector] public bool attacking;                            //Are we attacking?
    [HideInInspector] public bool onGroundCombat;                       //Combat on ground?
    [HideInInspector] public bool onAirCombat;                          //Combat on air?

    [SerializeField] int comboLength = 2;                               //Self explanatory
    [SerializeField] float timeBetweenAttacks = 0.4f;                   //Self explanatory
    [SerializeField] float bonusTimeToEndComboRelation = 0.5f;          //Time to reset combo after an attack (relative to time between attacks)
    [SerializeField] float attackRememberDuration = 0.1f;               //Time to hold attack=true after input (for easy control)
    [SerializeField] float airAttackVImpulse = 10f;                     //Set this vSpeed updwards when char is falling and attacks (ninja style)

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

        attacking = false;
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
    /// Updates all animation related fields. Called from playerAnimation component right before updating animator.
    /// </summary>
    public void UpdateCombat()
    {
        //Save attack input. Allways
        if (playerInput.attack)
            attackRememberTime = Time.time + attackRememberDuration;

        //Attack if: Not attacking + Input + Enough time has passed since last attack
        isGonnaAttack = (!attacking && attackRememberTime > Time.time && Time.time > nextAttakTime);

        //We are sure player will attack. on ground - on air??
        if (isGonnaAttack)
        {
            if (playerMovement.grounded)
                onGroundCombat = true;
            else
            {
                onAirCombat = true;
                //Little vertical impulse when player is falling
                if (playerMovement.falling)
                    playerMovement.ReceiveSpeedImpulse(new Vector2(0f, airAttackVImpulse));
            }
        }

        //Reset combo if time for combo has ended
        if (Time.time > comboEndTime)
            comboStep = 1;
    }

    public void OnStateMachineEnter()
    {
        attacking = true;
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
        attacking = false;
        onGroundCombat = false;
        onAirCombat = false;
    }
}