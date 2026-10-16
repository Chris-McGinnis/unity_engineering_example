using UnityEngine;

public class PlayerIdleState : State
{
    private Player player;
        
    public PlayerIdleState(StateMachine stateMachine, Player player)
    {
        Machine = stateMachine;
        this.player = player;
    }

    public override void Enter()
    {
        Debug.Log("[FSM] Entering Idle State");
    }

    public override void Update()
    {
        if (player.HorizontalInput != 0f)
        {
            Machine.ChangeState(new PlayerRunState(Machine, player));
        }
    }

    public override void Exit()
    {
        Debug.Log("[FSM] Exiting Idle State");
    }
}