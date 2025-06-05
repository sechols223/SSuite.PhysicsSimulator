using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Ardalis.GuardClauses;
using SSuite.PhysicsSimulator.Core.ValueObjects;

namespace SSuite.PhysicsSimulator.Core.PhysicalObjects;

public abstract partial class PhysicalObject
{
    public bool IsStatic { get; set; }

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }

    [Range(0, float.MaxValue)]
    public Mass Mass { get; set; }

    /// <summary>
    /// For collision detection
    /// </summary>
    [Range(0, float.MaxValue)]
    public Radius Radius { get; set; }

    /// <summary>
    /// Bounciness
    /// </summary>
    [Range(0, float.MaxValue)]
    public Restitution Restitution { get; set; }

    [Range(0, float.MaxValue)]
    public Friction Friction { get; set; }

    public void ApplyForce(Vector2 force)
    {
        if (this.IsStatic)
        {
            return;
        }

        var resultingAcceleration = this.GetAccelerationFromAppliedForce(force);
        this.Acceleration += resultingAcceleration;
    }

    public void ApplyForce(Vector2 force, PhysicalObject applyingObject)
    {
        var relativeMass = applyingObject.Mass / this.Mass;
        var forceToApply = force * relativeMass;

        var resultingAcceleration = this.GetAccelerationFromAppliedForce(forceToApply);
        this.Acceleration += resultingAcceleration;
    }

    public void UpdateKinematicVectors(DeltaTime deltaTime)
    {
        Guard.Against.Negative(deltaTime);

        if (this.IsStatic)
        {
            return;
        }

        this.UpdatePosition(deltaTime);
        this.UpdateVelocity(deltaTime);
        this.ResetAcceleration();
    }

    public void ApplyGlobalForces(AirResistance airResistance, Vector2 gravity)
    {
        if (this.IsStatic)
        {
            return;
        }

        var force = gravity * this.Mass;
        this.ApplyForce(force);

        var airResistanceForce = -this.Velocity * airResistance;
        this.ApplyForce(airResistanceForce);
    }

    private void UpdateVelocity(DeltaTime deltaTime)
    {
        Guard.Against.Negative(deltaTime);
        this.Velocity += this.Acceleration * deltaTime;
    }

    private void UpdatePosition(DeltaTime deltaTime)
    {
        Guard.Against.Negative(deltaTime);
        this.Position += this.Velocity * deltaTime;
    }

    private void ResetAcceleration()
        => this.Acceleration = Vector2.Zero;

    private Vector2 GetAccelerationFromAppliedForce(Vector2 force)
        => force / this.Mass;
}
