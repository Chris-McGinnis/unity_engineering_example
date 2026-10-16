using UnityEngine;

public class PlayerRunState : State
{
    private readonly Player player;
    
    public PlayerRunState(StateMachine stateMachine, Player player)
    {
        Machine = stateMachine;
        this.player = player;
    }

    public override void Enter()
    {
        Debug.Log("[FSM] Entering Run State");
    }

    public override void Update()
    {
        if (player.JumpPressed && player.IsGrounded)
        {
            player.RequestJump();
            Machine.ChangeState(new PlayerJumpState(Machine, player));
        }
        else if (player.HorizontalInput == 0f)
        {
            Machine.ChangeState(new PlayerIdleState(Machine, player));
        }
    }

    public override void Exit()
    {
        Debug.Log("[FSM] Exiting Run State");
    }
}
