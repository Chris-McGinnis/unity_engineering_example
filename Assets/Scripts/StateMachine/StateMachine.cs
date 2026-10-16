/// <summary>
/// Contract for individual states managed by a State Machine.
/// </summary>
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

/// <summary>
/// Generic state runner that executes the active state and guarantees deterministic transitions.
/// </summary>
public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        CurrentState?.Enter();
    }

    public void ChangeState(IState newState)
    {
        if (newState == null || newState == CurrentState) return;

        CurrentState?.Exit(); // Guarantee cleanup of old state
        CurrentState = newState;
        CurrentState?.Enter(); // Guarantee initialization of new state
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}