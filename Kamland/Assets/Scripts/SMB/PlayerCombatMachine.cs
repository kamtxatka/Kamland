using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatMachine : StateMachineBehaviour
{
    [HideInInspector] public PlayerCombat playerCombat;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Debug.Log("On state enter");
        playerCombat.OnStateEnter();
    }

    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        // Debug.Log("On state machine enter");
        playerCombat.OnStateMachineEnter();
    }

    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        // Debug.Log("On state machine exit");
        playerCombat.OnStateMachineExit();
    }

    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
}