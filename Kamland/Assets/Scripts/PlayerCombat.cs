using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [HideInInspector] public bool onCombat;
    [HideInInspector] public int comboStep = 1;
    [HideInInspector] public bool attack;

    [SerializeField] float comboLength = 2;
    [SerializeField] float timeBetweenAttacks = 0.3f;              //Time (seconds) between each attack
    [SerializeField] float bonusTimeToEndComboRelation = 1f;
    [SerializeField] float attackRememberDuration = 0.1f;

    PlayerInput input;
    Animator animator;
    PlayerCombatMachine playerCombatMachine;

    float nextAttakTime;
    float comboEndTime;
    float attackRememberTime;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        onCombat = false;
        comboStep = 1;
    }

    void Start()
    {
        playerCombatMachine = animator.GetBehaviour<PlayerCombatMachine>();
        playerCombatMachine.playerCombat = this;
    }

    public void UpdateCombos()
    {
        //2 conditions to attack
        //Input
        //It has been enough time since last attack
        if (input.attack)
            attackRememberTime = Time.time + attackRememberDuration;

        attack = (attackRememberTime > Time.time && Time.time > nextAttakTime);

        //Reset combo if time for combo has ended
        if (Time.time > comboEndTime)
            comboStep = 1;
    }

    public void OnStateMachineEnter()
    {
        onCombat = true;
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
        onCombat = false;
    }
}