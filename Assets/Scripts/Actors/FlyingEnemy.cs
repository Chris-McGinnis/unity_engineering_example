using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [SerializeField] private float hoverHeight = 3f;
    
    [Space(8)]
    // [SerializeReference] allows Unity to draw polymorphic C# classes in the Inspector
    [SerializeReference] private IAttackBehavior attackBehavior = new MeleeAttack();
    
    protected override void Awake()
    {
        base.Awake();
        if (body != null)
            body.gravityScale = 0f;
    }

    private void Update()
    {
        if (player == null)
        {
            TryFindPlayer();
        }
        
    }

    private void FixedUpdate()
    {
        if (body == null || IsPooled)
            return;

        if (player == null)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = new Vector2(player.position.x, player.position.y + hoverHeight);
        Vector2 offset = target - (Vector2)transform.position;
        body.linearVelocity = offset.magnitude > stoppingDistance
            ? offset.normalized * moveSpeed
            : Vector2.zero;
    }
    
    //Below is an example of how to use [SerializeReference] to allow polymorphic behavior in Unity's Inspector.
    // 1. Interface definition
    public interface IAttackBehavior
    {
        void ExecuteAttack();
    }
    
    // 2. Pure C# implementations (MUST have [Serializable])
    [System.Serializable]
    public class MeleeAttack : IAttackBehavior
    {
        [SerializeField] private float damage = 25f;
        public void ExecuteAttack() => Debug.Log($"Melee strike for {damage} damage!");
    }

    [System.Serializable]
    public class RangedAttack : IAttackBehavior
    {
        [SerializeField] private float range = 50f;
        public void ExecuteAttack() => Debug.Log($"Ranged arrow shot up to {range}m!");
    }
    
    public void PerformAttack()
    {
        attackBehavior?.ExecuteAttack();
    }
    
}
