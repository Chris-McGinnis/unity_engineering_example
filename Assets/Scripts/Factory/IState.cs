/// <summary>
/// Base class for states. The owning StateMachine attaches itself before Enter is called.
/// </summary>
public abstract class State : IState
{
    public StateMachine Machine { get; protected set; }
    
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}