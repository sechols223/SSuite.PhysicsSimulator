namespace SSuite.PhysicsSimulator.Core.ValueObjects;

public readonly record struct Restitution(float Value)
{
    public static implicit operator float(Restitution restitution) => restitution.Value;
    public static implicit operator Restitution(float value) => new(value);
}
