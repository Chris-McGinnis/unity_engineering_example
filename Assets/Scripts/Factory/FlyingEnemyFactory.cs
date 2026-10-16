using UnityEngine;

[CreateAssetMenu(fileName = "FlyingEnemyFactory", menuName = "McG/Factories/Flying Enemy Factory")]
public class FlyingEnemyFactory : EnemyFactory
{    
    [Header("Flying Enemy Configuration")] [SerializeField, Min(1f)]
    private float hoverHeight = 3f;
    
    protected override bool SupportsEnemy(EnemyBase enemy)
    {
        return enemy is FlyingEnemy;
    }
}
