using UnityEngine;

[CreateAssetMenu(fileName = "BasicEnemyFactory", menuName = "McG/Factories/Basic Enemy Factory")]
public class BasicEnemyFactory : EnemyFactory
{
    protected override bool SupportsEnemy(EnemyBase enemy)
    {
        return enemy is BasicEnemy;
    }
}