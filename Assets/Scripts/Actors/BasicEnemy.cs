using UnityEngine;

public class BasicEnemy : EnemyBase
{

    private void FixedUpdate()
    {
        if (body == null || IsPooled)
            return;

        if (player == null && !TryFindPlayer())
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        float horizontalDistance = player.position.x - transform.position.x;
        float horizontalSpeed = Mathf.Abs(horizontalDistance) > stoppingDistance
            ? Mathf.Sign(horizontalDistance) * moveSpeed
            : 0f;
        body.linearVelocity = new Vector2(horizontalSpeed, body.linearVelocity.y);
    }
}
