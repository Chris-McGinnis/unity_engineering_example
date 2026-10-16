using UnityEngine;

public class PlayerJumpState : State
{
    private readonly Player player;

    public PlayerJumpState(StateMachine machine, Player player)
    {
        Machine = machine;
        this.player = player;
    }

    public override void Enter()
    {
        Debug.Log("[FSM] Entering Jump State");
    }

    public override void Update()
    {
        if (player.IsGrounded)
        {
            IState nextState = player.HorizontalInput == 0f
                ? (IState)new PlayerIdleState(Machine, player)
                : new PlayerRunState(Machine, player);
            Machine.ChangeState(nextState);
        }
    }

    public override void Exit()
    {
        Debug.Log("[FSM] Exiting Jump State");
    }
}
