namespace SSuite.PhysicsSimulator.Core.ValueObjects;

public readonly record struct Mass(float Value)
{
    public static implicit operator float(Mass mass) => mass.Value;
    public static implicit operator Mass(float value) => new(value);
}
