namespace SSuite.PhysicsSimulator.Core.ValueObjects;

public readonly record struct Radius(float Value)
{
    public static implicit operator float(Radius radius) => radius.Value;
    public static implicit operator Radius(float value) => new(value);
}
