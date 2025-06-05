namespace SSuite.PhysicsSimulator.Core.ValueObjects;

public readonly record struct AirResistance(float Value)
{
    public static implicit operator float(AirResistance airResistance) => airResistance.Value;
    public static implicit operator AirResistance(float value) => new(value);
}
