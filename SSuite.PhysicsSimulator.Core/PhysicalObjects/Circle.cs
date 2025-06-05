using System.Numerics;

namespace SSuite.PhysicsSimulator.Core.PhysicalObjects;

public class Circle : PhysicalObject
{
    public override void ResolveCollision(PhysicalObject collidingObject)
    {
        if (collidingObject is not Circle collidingCircle)
        {
            base.ResolveCollision(collidingObject);
            return;
        }

        // Simple collision detection between circles
        var distance = Vector2.Distance(this.Position, collidingCircle.Position);
        var minDistance = this.Radius.Value + collidingCircle.Radius.Value;

        if (distance >= minDistance)
        {
            return;
        }

        var direction = Vector2.Normalize(this.Position - collidingCircle.Position);
        var overlap = minDistance - distance;

        this.Position += direction * overlap * 0.5f;
        collidingCircle.Position -= direction * overlap * 0.5f;

        var relativeVelocity = this.Velocity - collidingCircle.Velocity;
        var dot = Vector2.Dot(relativeVelocity, direction);

        if (dot >= 0)
        {
            return;
        }

        var impulse = -2 * dot / (this.Mass + collidingCircle.Mass);
        this.Velocity += direction * impulse * collidingCircle.Mass;
        collidingCircle.Velocity -= direction * impulse * this.Mass;

    }
}
