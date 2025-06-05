using System.Numerics;

namespace SSuite.PhysicsSimulator.Core.PhysicalObjects;

/*
 * NOTE: This logic was moved into a partial because of how verbose it is
 * There was probably a better way to do it, but it gave me an excuse to use partial classes
*/
public abstract partial class PhysicalObject
{
    public virtual void ResolveCollision(PhysicalObject collidingObject)
    {
        var distance = this.Position - collidingObject.Position;
        var distanceLength = distance.Length();
        var combinedRadius = this.Radius + collidingObject.Radius;

        if (distanceLength >= combinedRadius)
        {
            return;
        }

        var normal = distanceLength > 0 ? Vector2.Normalize(distance) : new Vector2(1, 0);

        var inverseMassA = this.IsStatic ? 0 : 1 / this.Mass;
        var inverseMassB = collidingObject.IsStatic ? 0 : 1 / collidingObject.Mass;
        var inverseMassSum = inverseMassA + inverseMassB;

        var velocityParams = new ResolveVelocitiesParams(this,
            collidingObject,
            normal,
            inverseMassA,
            inverseMassB,
            inverseMassSum);
        this.ResolveVelocities(velocityParams);

        var positionParams = new CorrectPositionsParams(this,
            collidingObject,
            normal,
            distanceLength,
            combinedRadius,
            inverseMassA, inverseMassB, inverseMassSum);
        this.CorrectPositions(positionParams);

        var frictionParams = new ApplyFrictionParams(this,
            collidingObject,
            normal,
            inverseMassA,
            inverseMassB,
            inverseMassSum);
        this.ApplyFriction(frictionParams);
    }

    private void ResolveVelocities(ResolveVelocitiesParams @params)
    {
        var relativeVelocity = @params.ObjectA.Velocity - @params.ObjectB.Velocity;
        var velocityAlongNormal = Vector2.Dot(relativeVelocity, @params.Normal);

        if (velocityAlongNormal > 0)
        {
            return;
        }

        var restitution = Math.Min(@params.ObjectA.Restitution, @params.ObjectB.Restitution);
        var impulseScalar = -(1 + restitution) * velocityAlongNormal / @params.InverseMassSum;
        var impulse = impulseScalar * @params.Normal;

        if (!@params.ObjectA.IsStatic)
        {
            @params.ObjectA.Velocity += impulse * @params.InverseMassA;
        }

        if (!@params.ObjectB.IsStatic)
        {
            @params.ObjectB.Velocity -= impulse * @params.InverseMassB;
        }
    }

    private void CorrectPositions(CorrectPositionsParams @params)
    {
        const float positionCorrectionPercent = 0.2f;
        const float penetrationSlop = 0.01f;

        var penetration = @params.CombinedRadius - @params.DistanceLength;

        if (penetration > penetrationSlop)
        {
            var correction = (penetration - penetrationSlop) / @params.InverseMassSum * positionCorrectionPercent * @params.Normal;

            if (!@params.ObjectA.IsStatic)
            {
                @params.ObjectA.Position += correction * @params.InverseMassA;
            }

            if (!@params.ObjectB.IsStatic)
            {
                @params.ObjectB.Position -= correction * @params.InverseMassB;
            }
        }
    }

    private void ApplyFriction(ApplyFrictionParams @params)
    {
        var relativeVelocity = @params.ObjectA.Velocity - @params.ObjectB.Velocity;
        var normalVelocity = Vector2.Dot(relativeVelocity, @params.Normal) * @params.Normal;
        var tangentialVelocity = relativeVelocity - normalVelocity;

        if (tangentialVelocity == Vector2.Zero)
        {
            return;
        }

        var tangent = Vector2.Normalize(tangentialVelocity);
        var frictionImpulseScalar = -Vector2.Dot(relativeVelocity, tangent) / @params.InverseMassSum;
        var frictionCoefficient = (@params.ObjectA.Friction + @params.ObjectB.Friction) * 0.5f;

        var normalImpulseScalar = -(1 + Math.Min(@params.ObjectA.Restitution, @params.ObjectB.Restitution)) *
                                   Vector2.Dot(relativeVelocity, @params.Normal) / @params.InverseMassSum;

        var frictionImpulse = Math.Min(Math.Abs(frictionImpulseScalar),
            Math.Abs(normalImpulseScalar) * frictionCoefficient) * tangent;

        if (!@params.ObjectA.IsStatic)
        {
            @params.ObjectA.Velocity -= frictionImpulse * @params.InverseMassA;
        }

        if (!@params.ObjectB.IsStatic)
        {
            @params.ObjectB.Velocity += frictionImpulse * @params.InverseMassB;
        }
    }
}

// Record for ResolveVelocities parameters
internal sealed record ResolveVelocitiesParams(
    PhysicalObject ObjectA,
    PhysicalObject ObjectB,
    Vector2 Normal,
    float InverseMassA,
    float InverseMassB,
    float InverseMassSum);

// Record for CorrectPositions parameters
internal sealed record CorrectPositionsParams(
    PhysicalObject ObjectA,
    PhysicalObject ObjectB,
    Vector2 Normal,
    float DistanceLength,
    float CombinedRadius,
    float InverseMassA,
    float InverseMassB,
    float InverseMassSum);

// Record for ApplyFriction parameters
internal sealed record ApplyFrictionParams(
    PhysicalObject ObjectA,
    PhysicalObject ObjectB,
    Vector2 Normal,
    float InverseMassA,
    float InverseMassB,
    float InverseMassSum);

